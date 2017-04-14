using System;
using System.Collections.Concurrent;
using System.Threading;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// A cancellable queuing consumer
    /// </summary>
    internal class CancellableQueueingConsumer : DefaultBasicConsumer
    {
        private readonly CancellationToken cancellationToken;
        private readonly BlockingCollection<BasicDeliverEventArgs> queue = new BlockingCollection<BasicDeliverEventArgs>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableQueueingConsumer"/> class. 
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

        /// <summary>
        /// The dequeue.
        /// </summary>
        /// <returns>
        /// The <see cref="RabbitMQ.Client.Events.BasicDeliverEventArgs"/>.
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
    }
}
