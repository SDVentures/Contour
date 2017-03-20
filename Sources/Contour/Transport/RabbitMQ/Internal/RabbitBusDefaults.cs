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

        /// <summary>
        /// Defines a default producer selector as Round Robin selector
        /// </summary>
        public static IProducerSelector ProducerSelector = new RoundRobinSelector();

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

            Queue queue = builder.Topology.Declare(
                Queue.Named(queueName).
                    Durable);
            Exchange exchange = builder.Topology.Declare(
                Exchange.Named(label).
                    Durable.Fanout);

            builder.Topology.Bind(exchange, queue);

            return builder.ListenTo(queue, exchange);
        }
    }
}
