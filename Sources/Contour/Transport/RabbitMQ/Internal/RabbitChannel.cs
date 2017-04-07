﻿namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Common.Logging;
    using Helpers;
    using Receiving;
    using Topology;
    using global::RabbitMQ.Client;

    /// <summary>
    /// The rabbit channel.
    /// </summary>
    internal sealed class RabbitChannel : IChannel
    {
        private readonly object sync = new object();
        private readonly IBusContext busContext;
        private readonly ILog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitChannel"/> class. 
        /// </summary>
        /// <param name="connectionId">A connection identifier to which this channel belongs</param>
        /// <param name="model">A native transport channel</param>
        /// <param name="busContext">A bus context</param>
        public RabbitChannel(Guid connectionId, IModel model, IBusContext busContext)
        {
            this.ConnectionId = connectionId;
            this.Model = model;
            this.busContext = busContext;
            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.ConnectionId}, {this.GetHashCode()})");

            this.Model.ModelShutdown += this.OnModelShutdown;
        }

        public Guid ConnectionId { get; }

        /// <summary>
        /// The failed.
        /// </summary>
        [Obsolete("Channel failures are no longer propagated outside via events, instead an exception is thrown")]
        public event Action<IChannel, ErrorEventArgs> Failed = (channel, args) => { };

        /// <summary>
        /// Is fired on channel shutdown.
        /// </summary>
        public event Action<IChannel, ShutdownEventArgs> Shutdown = (channel, args) => { };
        
        /// <summary>
        /// Gets the native.
        /// </summary>
        protected IModel Model { get; }

        /// <summary>
        /// Aborts the channel
        /// </summary>
        public void Abort()
        {
            this.Model?.Abort();
        }

        /// <summary>
        /// The accept.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        public void Accept(RabbitDelivery delivery)
        {
            this.logger.Trace(m => m("Accepting message [{0}] ({1}).", delivery.Label, delivery.Args.DeliveryTag));

            this.SafeNativeInvoke(n => n.BasicAck(delivery.Args.DeliveryTag, false));
        }

        /// <summary>
        /// The bind.
        /// </summary>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        public void Bind(Queue queue, Exchange exchange, string routingKey)
        {
            this.SafeNativeInvoke(n => n.QueueBind(queue.Name, exchange.Name, routingKey));
        }

        /// <summary>
        /// The build cancellable consumer.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="CancellableQueueingConsumer"/>.
        /// </returns>
        public CancellableQueueingConsumer BuildCancellableConsumer(CancellationToken cancellationToken)
        {
            return new CancellableQueueingConsumer(this.Model, cancellationToken);
        }

        /// <summary>
        /// The build queuing consumer.
        /// </summary>
        /// <returns>
        /// The <see cref="QueueingBasicConsumer"/>.
        /// </returns>
        public QueueingBasicConsumer BuildQueueingConsumer()
        {
            var consumer = new QueueingBasicConsumer(this.Model);
            return consumer;
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        public void Declare(Exchange exchange)
        {
            this.SafeNativeInvoke(n => n.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.AutoDelete, new Dictionary<string, object>()));
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="queue">
        /// The queue.
        /// </param>
        public void Declare(Queue queue)
        {
            var arguments = new Dictionary<string, object>();
            if (queue.Ttl.HasValue)
            {
                arguments.Add(Headers.QueueMessageTtl, (long)queue.Ttl.Value.TotalMilliseconds);
            }
            if (queue.Limit.HasValue)
            {
                arguments.Add(Headers.QueueMaxLength, (int)queue.Limit);
            }


            this.SafeNativeInvoke(n => n.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete, arguments));
        }

        /// <summary>
        /// The declare default queue.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string DeclareDefaultQueue()
        {
            string queueName = string.Empty;

            this.SafeNativeInvoke(n => queueName = n.QueueDeclare());

            return queueName;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.Model != null)
            {
                this.Model.ModelShutdown -= this.OnModelShutdown;

                try
                {
                    this.Model.Abort();
                    this.Model.Dispose();
                }
                catch (Exception)
                {
                    // Suppress all errors here
                }
            }
        }

        /// <summary>
        /// Enable publish confirmation.
        /// </summary>
        public void EnablePublishConfirmation()
        {
            this.SafeNativeInvoke(n => n.ConfirmSelect());
        }

        /// <summary>
        /// Get next seq no.
        /// </summary>
        /// <returns>
        /// The <see cref="ulong"/>.
        /// </returns>
        public ulong GetNextSeqNo()
        {
            ulong seqNo = 0UL;
            this.SafeNativeInvoke(n => seqNo = n.NextPublishSeqNo);
            return seqNo;
        }

        /// <summary>
        /// The on confirmation.
        /// </summary>
        /// <param name="handleConfirmation">
        /// The handle confirmation.
        /// </param>
        public void OnConfirmation(ConfirmationHandler handleConfirmation)
        {
            this.SafeNativeInvoke(
                n =>
                    {
                        n.BasicAcks += (model, args) => handleConfirmation(true, args.DeliveryTag, args.Multiple);
                        n.BasicNacks += (model, args) => handleConfirmation(false, args.DeliveryTag, args.Multiple);
                    });
        }

        /// <summary>
        /// The publish.
        /// </summary>
        /// <param name="route">
        /// The route.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="propsVisitor">
        /// The props visitor.
        /// </param>
        public void Publish(IRoute route, IMessage message, Func<IBasicProperties, IDictionary<string, object>> propsVisitor = null)
        {
            var nativeRoute = (RabbitRoute)route;

            this.logger.Trace(m => m("Emitting message [{0}] ({1}) through [{2}].", message.Label, message.Payload, nativeRoute));

            var props = this.Model.CreateBasicProperties();
            var body = this.busContext.PayloadConverter.FromObject(message.Payload);

            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            props.ContentType = this.busContext.PayloadConverter.ContentType;
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());

            var headers = propsVisitor?.Invoke(props);
            headers.ForEach(i => props.Headers.Add(i));

            this.busContext.MessageLabelHandler.Inject(props, message.Label);
            this.SafeNativeInvoke(n => n.BasicPublish(nativeRoute.Exchange, nativeRoute.RoutingKey, false, props, body));
        }

        /// <summary>
        /// The reject.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        /// <param name="requeue">
        /// The requeue.
        /// </param>
        public void Reject(RabbitDelivery delivery, bool requeue)
        {
            this.logger.Trace(m => m("Rejecting message [{0}] ({1}).", delivery.Label, delivery.Args.DeliveryTag));

            this.SafeNativeInvoke(n => n.BasicNack(delivery.Args.DeliveryTag, false, requeue));
        }

        /// <summary>
        /// The reply.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="replyTo">
        /// The reply to.
        /// </param>
        /// <param name="correlationId">
        /// The correlation id.
        /// </param>
        public void Reply(IMessage message, RabbitRoute replyTo, string correlationId)
        {
            Func<IBasicProperties, IDictionary<string, object>> propsVisitor = p =>
            {
                p.CorrelationId = correlationId;
                return message.Headers;
            };

            this.Publish(new RabbitRoute(replyTo.Exchange, replyTo.RoutingKey), message, propsVisitor);
        }

        /// <summary>
        /// The request publish.
        /// </summary>
        /// <param name="qos">
        /// The qos.
        /// </param>
        public void RequestPublish(QoSParams qos)
        {
            this.SafeNativeInvoke(n => n.BasicQos(qos.PrefetchSize, qos.PrefetchCount, false));
        }

        /// <summary>
        /// The set qos.
        /// </summary>
        /// <param name="qos">
        /// The qos.
        /// </param>
        public void SetQos(QoSParams qos)
        {
            this.SafeNativeInvoke(n => n.BasicQos(qos.PrefetchSize, qos.PrefetchCount, false));
        }

        /// <summary>
        /// The start consuming.
        /// </summary>
        /// <param name="listeningSource">
        /// The listening source.
        /// </param>
        /// <param name="requireAccept">
        /// The require accept.
        /// </param>
        /// <param name="consumer">
        /// The consumer.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string StartConsuming(IListeningSource listeningSource, bool requireAccept, IBasicConsumer consumer)
        {
            string consumerTag = string.Empty;

            this.SafeNativeInvoke(n => consumerTag = n.BasicConsume(listeningSource.Address, !requireAccept, consumer));

            return consumerTag;
        }

        /// <summary>
        /// The stop consuming.
        /// </summary>
        /// <param name="consumerTag">
        /// The consumer tag.
        /// </param>
        public void StopConsuming(string consumerTag)
        {
            this.SafeNativeInvoke(n => n.BasicCancel(consumerTag));
        }

        /// <summary>
        /// The try stop consuming.
        /// </summary>
        /// <param name="consumerTag">
        /// The consumer tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool TryStopConsuming(string consumerTag)
        {
            try
            {
                this.Model.BasicCancel(consumerTag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The unpack as.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        /// <returns>
        /// The <see cref="IMessage"/>.
        /// </returns>
        public IMessage UnpackAs(Type type, RabbitDelivery delivery)
        {
            var payload = this.busContext.PayloadConverter.ToObject(delivery.Args.Body, type);
            return new Message(delivery.Label, delivery.Headers, payload);
        }

        private void OnModelShutdown(IModel model, ShutdownEventArgs args)
        {
            this.logger.Trace($"Channel is closed due to '{args.ReplyText}'");
            this.Shutdown(this, args);
        }

        /// <summary>
        /// The safe native invoke.
        /// </summary>
        /// <param name="invokeAction">
        /// The invoke action.
        /// </param>
        private void SafeNativeInvoke(Action<IModel> invokeAction)
        {
            try
            {
                lock (this.sync)
                {
                    this.logger.Trace($"Performing channel action [{invokeAction.Method.Name}]");
                    invokeAction(this.Model);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Channel action failed due to {ex.Message}", ex);
                throw;
            }
        }
    }
}
