namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;

    using Common.Logging;

    using Contour.Helpers;
    using Contour.Receiving;
    using Contour.Transport.RabbitMQ.Topology;

    using global::RabbitMQ.Client;

    using global::RabbitMQ.Client.Exceptions;

    /// <summary>
    /// The rabbit channel.
    /// </summary>
    internal class RabbitChannel : IChannel
    {
        #region Static Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The _bus.
        /// </summary>
        private readonly IRabbitBus bus;

        /// <summary>
        /// The _sync.
        /// </summary>
        private readonly object sync = new object();

        /// <summary>
        /// Is closed?
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private bool isClosed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitChannel"/>.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="model">
        /// The Model.
        /// </param>
        public RabbitChannel(IRabbitBus bus, IModel model)
        {
            this.bus = bus;
            this.Model = model;

            Logger.TraceFormat("Channel is opened.");

            this.Model.ModelShutdown += (m, reason) =>
                {
                    this.isClosed = true;
                    Logger.TraceFormat("Channel is closed due to \"{0}\".", reason.ToString());
                };
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The failed.
        /// </summary>
        public event Action<IChannel, ErrorEventArgs> Failed = (channel, args) => { };

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the bus.
        /// </summary>
        public IBusContext Bus
        {
            get
            {
                return this.bus;
            }
        }

        public bool IsOpen
        {
            get
            {
                return (Model != null && Model.IsOpen);
            }
        }

        #endregion

        // leave protected to have a full control over Channel ops
        #region Properties

        /// <summary>
        /// Gets the Model.
        /// </summary>
        protected IModel Model { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The accept.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        public void Accept(RabbitDelivery delivery)
        {
            Logger.Trace(m => m("Accepting message [{0}] ({1}).", delivery.Label, delivery.Args.DeliveryTag));

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
        /// The build queueing consumer.
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
                if (this.bus.Connection != null && this.bus.Connection.CloseReason != null)
                {
                    return;
                }

                if (this.Model.IsOpen && !this.isClosed)
                {
                    this.Model.Close();
                }

                this.Model.Dispose();
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
        public void Publish(IRoute route, IMessage message, Action<IBasicProperties> propsVisitor = null)
        {
            var nativeRoute = (RabbitRoute)route;

            Logger.Trace(m => m("Emitting message [{0}] ({1}) through [{2}].", message.Label, message.Payload, nativeRoute));

            IBasicProperties props = this.Model.CreateBasicProperties();
            byte[] body = this.Bus.PayloadConverter.FromObject(message.Payload);

            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            props.ContentType = this.Bus.PayloadConverter.ContentType;
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());

            if (propsVisitor != null)
            {
                propsVisitor(props);
            }

            message.Headers.ForEach(i => props.Headers.Add(i));

            this.Bus.MessageLabelHandler.Inject(props, message.Label);

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
            Logger.Trace(m => m("Rejecting message [{0}] ({1}).", delivery.Label, delivery.Args.DeliveryTag));

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
            Action<IBasicProperties> propsVisitor = props => { props.CorrelationId = correlationId; };

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
            object payload = this.Bus.PayloadConverter.ToObject(delivery.Args.Body, type);

            return new Message(delivery.Label, delivery.Headers, payload);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The safe Model invoke.
        /// </summary>
        /// <param name="invokeAction">
        /// The invoke action.
        /// </param>
        private void SafeNativeInvoke(Action<IModel> invokeAction)
        {
            try
            {
                if (!this.isClosed)
                {
                    lock (this.sync)
                        invokeAction(this.Model);
                }
            }
            catch (AlreadyClosedException ex)
            {
                Logger.Warn("Cought exception. Marking Channel as failed.", ex);

                this.Failed(this, new ErrorEventArgs(ex));
            }
            catch (OperationInterruptedException ex)
            {
                Logger.Trace("Cought exception. Ignoring as part of shutdown process.", ex);

                this.Failed(this, new ErrorEventArgs(ex));
            }
        }

        #endregion
    }
}
