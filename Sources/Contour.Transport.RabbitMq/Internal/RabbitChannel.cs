using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Common.Logging;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Serialization;
using Contour.Transport.RabbitMq.Topology;

using RabbitMQ.Client;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// The rabbit channel.
    /// </summary>
    internal sealed class RabbitChannel : IRabbitChannel
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
        /// Is fired on channel shutdown.
        /// </summary>
        public event Action<IChannel, ShutdownEventArgs> Shutdown = (channel, args) => { };
        
        /// <summary>
        /// Gets the native.
        /// </summary>
        private IModel Model { get; }

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
        /// The <see cref="RabbitMQ.Client.QueueingBasicConsumer"/>.
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

        public void Publish(IRoute route, IMessage message, IPayloadConverter payloadConverter)
        {
            var nativeRoute = (RabbitRoute)route;

            this.logger.Trace(m => m("Emitting message [{0}] ({1}) through [{2}].", message.Label, message.Payload, nativeRoute));

            var props = this.Model.CreateBasicProperties();

            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            var headers = ExtractProperties(props, message.Headers);
            var body = payloadConverter.FromObject(message.Payload);

            props.ContentType = payloadConverter.ContentType;
            props.Timestamp = new AmqpTimestamp(DateTimeEx.ToUnixTimestamp(DateTime.UtcNow));

            headers.ForEach(i => props.Headers.Add(i));

            this.busContext.MessageLabelHandler.Inject(props, message.Label);
            this.SafeNativeInvoke(n => n.BasicPublish(nativeRoute.Exchange, nativeRoute.RoutingKey, false, props, body));
        }

        public void Reject(RabbitDelivery delivery, bool requeue)
        {
            this.logger.Trace(m => m("Rejecting message [{0}] ({1}).", delivery.Label, delivery.Args.DeliveryTag));

            this.SafeNativeInvoke(n => n.BasicNack(delivery.Args.DeliveryTag, false, requeue));
        }

        public void RequestPublish(QoSParams qos)
        {
            this.SafeNativeInvoke(n => n.BasicQos(qos.PrefetchSize, qos.PrefetchCount, false));
        }

        public void SetQos(QoSParams qos)
        {
            this.SafeNativeInvoke(n => n.BasicQos(qos.PrefetchSize, qos.PrefetchCount, false));
        }

        public string StartConsuming(IListeningSource listeningSource, bool requireAccept, IBasicConsumer consumer)
        {
            string consumerTag = string.Empty;

            this.SafeNativeInvoke(n => consumerTag = n.BasicConsume(listeningSource.Address, !requireAccept, consumer));

            return consumerTag;
        }

        public void StopConsuming(string consumerTag)
        {
            this.SafeNativeInvoke(n => n.BasicCancel(consumerTag));
        }

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

        public IMessage UnpackAs(Type type, RabbitDelivery delivery, IPayloadConverter payloadConverter)
        {
            var payload = payloadConverter.ToObject(delivery.Args.Body, type);

            return new Message(delivery.Label, delivery.Headers, payload);
        }

        private void OnModelShutdown(IModel model, ShutdownEventArgs args)
        {
            this.logger.Trace($"Channel is closed due to '{args.ReplyText}'");
            this.Shutdown(this, args);
        }

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

        private static IDictionary<string, object> ExtractProperties(
            IBasicProperties props, 
            IDictionary<string, object> sourceHeaders)
        {
            var headers = new Dictionary<string, object>(sourceHeaders);

            var persist = Headers.Extract<bool?>(headers, Headers.Persist);
            var ttl = Headers.Extract<TimeSpan?>(headers, Headers.Ttl);
            var correlationId = Headers.Extract<string>(headers, Headers.CorrelationId);
            var replyRoute = Headers.Extract<RabbitRoute>(headers, Headers.ReplyRoute);

            if (persist.HasValue && persist.Value)
            {
                props.DeliveryMode = 2;
            }

            if (ttl.HasValue)
            {
                props.Expiration = ttl.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            if (correlationId != null)
            {
                props.CorrelationId = correlationId;
            }

            if (replyRoute != null)
            {
                props.ReplyToAddress = new PublicationAddress("direct", replyRoute.Exchange, replyRoute.RoutingKey);
            }

            return headers;
        }
    }
}
