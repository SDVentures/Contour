using System;
using Contour.Receiving;
using Contour.Sending;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// The rabbit bus defaults.
    /// </summary>
    internal static class RabbitBusDefaults
    {
        /// <summary>
        /// The route resolver builder.
        /// </summary>
        public static Func<IRouteResolverBuilder, IRouteResolver> RouteResolverBuilder = RouteResolverBuilderImpl;

        /// <summary>
        /// The subscription endpoint builder.
        /// </summary>
        public static Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> SubscriptionEndpointBuilder = SubscriptionEndpointBuilderImpl;

        public static IProducerSelectorBuilder ProducerSelectorBuilder = new DefaultProducerSelectorBuilder();

        /// <summary>
        /// The route resolver builder impl.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <returns>
        /// The <see cref="IRouteResolver"/>.
        /// </returns>
        private static IRouteResolver RouteResolverBuilderImpl(IRouteResolverBuilder builder)
        {
            string label = builder.Sender.Label.Name;

            Exchange exchange = builder.Topology.Declare(
                Exchange.Named(label).
                    Durable.Fanout);

            return new StaticRouteResolver(exchange);
        }

        /// <summary>
        /// The subscription endpoint builder impl.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <returns>
        /// The <see cref="ISubscriptionEndpoint"/>.
        /// </returns>
        private static ISubscriptionEndpoint SubscriptionEndpointBuilderImpl(ISubscriptionEndpointBuilder builder)
        {
            string label = builder.Receiver.Label.Name;

            string queueName = builder.Endpoint.Address + "." + label;

            var queueBuilder = Queue
                .Named(queueName)
                .Durable;

            var busOptions = (ReceiverOptions)builder.Receiver.Options.Parent;

            if (busOptions.GetQueueLimit().HasValue)
            {
                queueBuilder.WithLimit(busOptions.GetQueueLimit().Value);
            }

            if (busOptions.GetQueueMaxLengthBytes().HasValue)
            {
                queueBuilder.WithMaxLengthBytes(busOptions.GetQueueMaxLengthBytes().Value);
            }

            Queue queue = builder.Topology.Declare(queueBuilder);
            Exchange exchange = builder.Topology.Declare(
                Exchange.Named(label).
                    Durable.Fanout);

            builder.Topology.Bind(exchange, queue);

            return builder.ListenTo(queue, exchange);
        }
    }
}
