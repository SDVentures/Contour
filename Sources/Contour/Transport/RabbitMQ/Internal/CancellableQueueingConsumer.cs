namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using global::RabbitMQ.Client;

    using global::RabbitMQ.Client.Events;

    /// <summary>
    /// The cancellable queueing consumer.
    /// </summary>
    internal class CancellableQueueingConsumer : DefaultBasicConsumer
    {
        #region Fields

        /// <summary>
        /// The _cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        /// The _queue.
        /// </summary>
        private readonly BlockingCollection<BasicDeliverEventArgs> queue = new BlockingCollection<BasicDeliverEventArgs>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="CancellableQueueingConsumer"/>.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public CancellableQueueingConsumer(IModel model, CancellationToken cancellationToken)
            : base(model)
        {
            this.cancellationToken = cancellationToken;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dequeue.
        /// </summary>
        /// <returns>
        /// The <see cref="BasicDeliverEventArgs"/>.
        /// </returns>
        public BasicDeliverEventArgs Dequeue()
        {
            return this.queue.Take(this.cancellationToken);
        }

        /// <summary>
        /// The handle basic deliver.
        /// </summary>
        /// <param name="consumerTag">
        /// The consumer tag.
        /// </param>
        /// <param name="deliveryTag">
        /// The delivery tag.
        /// </param>
        /// <param name="redelivered">
        /// The redelivered.
        /// </param>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            try
            {
                this.queue.Add(new BasicDeliverEventArgs { Exchange = exchange, RoutingKey = routingKey, ConsumerTag = consumerTag, DeliveryTag = deliveryTag, Redelivered = redelivered, BasicProperties = properties, Body = body }, this.cancellationToken);
            }
            catch (InvalidOperationException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        #endregion
    }
}
