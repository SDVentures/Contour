using System.Threading;

using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Helpers;
    using Helpers.CodeContracts;

    using Sending;

    using Topology;

    /// <summary>
    /// The producer registry.
    /// </summary>
    internal class ProducerRegistry : IDisposable
    {
        private readonly ILog logger = LogManager.GetLogger<ProducerRegistry>();

        /// <summary>
        /// The _bus.
        /// </summary>
        private readonly RabbitBus bus;
        private readonly IConnectionPool<IRabbitConnection> pool; 

        /// <summary>
        /// The _producers.
        /// </summary>
        private readonly IDictionary<MessageLabel, Producer> producers = new ConcurrentDictionary<MessageLabel, Producer>();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProducerRegistry"/>.
        /// </summary>
        public ProducerRegistry(RabbitBus bus, IConnectionPool<IRabbitConnection> pool)
        {
            this.bus = bus;
            this.pool = pool;
        }
        

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.producers.Values.ForEach(l => l.Dispose());
            this.producers.Clear();
        }

        /// <summary>
        /// The reset.
        /// </summary>
        public void Reset()
        {
            {
                // lock (_producers)
                this.producers.Values.ForEach(l => l.Dispose());
                this.producers.Clear();
            }
        }

        /// <summary>
        /// The resolve for.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="Producer"/>.
        /// </returns>
        public Producer ResolveFor(ISenderConfiguration configuration)
        {
            var source = new CancellationTokenSource();
            var connection = this.pool.Get(source.Token);
            this.logger.Trace($"Using connection [{connection.Id}] to resolve a producer");

            var producer = this.TryResolverFor(configuration.Label);
            if (producer == null)
            {
                using (var channel = connection.OpenChannel())
                {
                    var topologyBuilder = new TopologyBuilder(channel);
                    var builder = new RouteResolverBuilder(this.bus.Endpoint, topologyBuilder, configuration);
                    var routeResolverBuilder = configuration.Options.GetRouteResolverBuilder();

                    Assumes.True(routeResolverBuilder.HasValue, "RouteResolverBuilder must be set for [{0}]", 
                        configuration.Label);

                    var routeResolver = routeResolverBuilder.Value(builder);
                    
                    producer = new Producer(connection, configuration.Label, routeResolver, 
                        configuration.Options.IsConfirmationRequired());
                    producer.Failed += p =>
                    {
                        {
                            p.Dispose();
                            this.producers.Remove(p.Label);
                        }
                    };

                    this.producers.Add(configuration.Label, producer);
                }

                if (configuration.RequiresCallback)
                {
                    producer.UseCallbackListener(
                        this.bus.ListenerRegistry.ResolveFor(configuration.CallbackConfiguration));
                }
            }

            return producer;
        }

        /// <summary>
        /// The try resolver for.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="Producer"/>.
        /// </returns>
        private Producer TryResolverFor(MessageLabel label)
        {
            Producer producer;
            this.producers.TryGetValue(label, out producer);
            return producer;
        }
    }
}
