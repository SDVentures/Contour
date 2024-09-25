using RabbitMQ.Client.Events;

namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        /// <param name="connectionString">Used connection string</param>
        /// <param name="connectionKey">Key by connection string</param>
        public RabbitChannel(Guid connectionId, IModel model, IBusContext busContext, string connectionString, string connectionKey)
        {
            this.ConnectionId = connectionId;
            this.Model = model;
            this.busContext = busContext;
            this.ConnectionString = connectionString;
            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.ConnectionId}, {this.GetHashCode()})");
            this.ConnectionKey = connectionKey;

            this.Model.ModelShutdown += this.OnModelShutdown;
        }

        /// <summary>
        /// Строка подключения
        /// </summary>
        internal string ConnectionString { get; }

        public Guid ConnectionId { get; }

        public string ConnectionKey { get; }

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
        public void Bind(Queue queue, Exchange exchange, string routingKey, IDictionary<string, object> arguments = null)
        {
            try
            {
                this.SafeNativeInvoke(n => n.QueueBind(queue.Name, exchange.Name, routingKey, arguments));
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed to bind queue to exchange [{1}:{2}] on channel: {0}", this.ToString(), exchange.Name, queue.Name), e);
                throw;
            }
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
        public AsyncEventingBasicConsumer BuildConsumer()
        {
            return new AsyncEventingBasicConsumer(this.Model);
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        public void Declare(Exchange exchange)
        {
            try
            {
                this.SafeNativeInvoke(n => n.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.AutoDelete, exchange.Arguments));
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed to Declare Exchange [{1}] on channel: {0}", this.ToString(), exchange.Name), e);
                throw;
            }
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="queue">
        /// The queue.
        /// </param>
        public void Declare(Queue queue)
        {
            try
            {
                var arguments = new Dictionary<string, object>();
                if (queue.Ttl.HasValue)
                {
                    arguments.Add(Contour.Headers.QueueMessageTtl, (long)queue.Ttl.Value.TotalMilliseconds);
                }

                if (queue.Limit.HasValue)
                {
                    arguments.Add(Contour.Headers.QueueMaxLength, (int)queue.Limit);
                }

                if (queue.MaxLengthBytes.HasValue)
                {
                    arguments.Add(Contour.Headers.QueueMaxLengthBytes, (int)queue.MaxLengthBytes);
                }

                this.SafeNativeInvoke(n =>
                    n.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete, arguments));
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed to Declare Queue [{1}] on channel: {0}", this.ToString(), queue.Name),
                    e);
                throw;
            }
        }

        /// <summary>
        /// The declare default queue.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string DeclareDefaultQueue()
        {
            try
            {
                var queueName = string.Empty;

                this.SafeNativeInvoke(n => queueName = n.QueueDeclare());

                return queueName;
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed to Declare Default Queue on channel: {0}", this.ToString()), e);
                throw;
            }
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

            DiagnosticProps.Store(DiagnosticProps.Names.LastPublishAttemptConnectionString, this.ConnectionString);

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
        /// The set qos.
        /// </summary>
        /// <param name="qos">
        /// The qos.
        /// </param>
        public void SetQos(QoSParams qos)
        {
            try
            {
                this.SafeNativeInvoke(n => n.BasicQos(qos.PrefetchSize, qos.PrefetchCount, false));
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed to set Qos on channel: {0}", this.ToString()), e);
                throw;
            }
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
            try
            {
                var consumerTag = string.Empty;

                this.SafeNativeInvoke(n => consumerTag = n.BasicConsume(listeningSource.Address, !requireAccept, consumer));

                return consumerTag;
            }
            catch (Exception e)
            {
                this.logger.Error(m => m("Failed start consuming on channel."), e);
                throw;
            }
        }

        /// <summary>
        /// Unsubscribe consumer from queue via <param name="tag"/> subscription ID.
        /// </summary>
        /// <param name="tag">Subscription ID</param>
        public void StopConsuming(string tag)
        {
            this.Model.BasicCancel(tag);
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
            var payload = this.busContext.PayloadConverter.ToObject(delivery.Args.Body.ToArray(), type);
            return new Message(delivery.Label, delivery.Headers, payload);
        }

        private void OnModelShutdown(object sender, ShutdownEventArgs args)
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
                this.logger.Error($"Channel action failed due to {ex.Message}, connection string: [{this.ConnectionString}]", ex);
                throw;
            }
        }
    }
}
