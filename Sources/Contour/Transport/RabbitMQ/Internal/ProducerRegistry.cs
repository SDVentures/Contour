namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Contour.Helpers;
    using Contour.Helpers.CodeContracts;
    using Contour.Sending;
    using Contour.Transport.RabbitMQ.Topology;

    /// <summary>
    /// The producer registry.
    /// </summary>
    internal class ProducerRegistry : IDisposable
    {
        #region Fields

        /// <summary>
        /// The _bus.
        /// </summary>
        private readonly RabbitBus _bus;

        /// <summary>
        /// The _producers.
        /// </summary>
        private readonly IDictionary<MessageLabel, Producer> _producers = new ConcurrentDictionary<MessageLabel, Producer>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProducerRegistry"/>.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        public ProducerRegistry(RabbitBus bus)
        {
            this._bus = bus;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this._producers.Values.ForEach(l => l.Dispose());
            this._producers.Clear();
        }

        /// <summary>
        /// The reset.
        /// </summary>
        public void Reset()
        {
            {
                // lock (_producers)
                this._producers.Values.ForEach(l => l.Dispose());
                this._producers.Clear();
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
            Producer producer = this.TryResolverFor(configuration.Label);
            if (producer == null)
            {
                using (RabbitChannel channel = this._bus.OpenChannel())
                {
                    var topologyBuilder = new TopologyBuilder(channel);
                    var builder = new RouteResolverBuilder(this._bus.Endpoint, topologyBuilder, configuration);
                    Maybe<Func<IRouteResolverBuilder, IRouteResolver>> routeResolverBuilder = configuration.Options.GetRouteResolverBuilder();

                    Assumes.True(routeResolverBuilder.HasValue, "RouteResolverBuilder must be set for [{0}]", configuration.Label);

                    IRouteResolver routeResolver = routeResolverBuilder.Value(builder);

                    producer = new Producer(this._bus, configuration.Label, routeResolver, configuration.Options.IsConfirmationRequired());
                    producer.Failed += p =>
                        {
                            {
                                // lock (_producers)
                                p.Dispose();
                                this._producers.Remove(p.Label);
                            }
                        };

                    // lock (_producers)
                    this._producers.Add(configuration.Label, producer);
                }

                if (configuration.RequiresCallback)
                {
                    producer.UseCallbackListener(this._bus.ListenerRegistry.ResolveFor(configuration.CallbackConfiguration));
                }
            }

            return producer;
        }

        #endregion

        #region Methods

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
            this._producers.TryGetValue(label, out producer);
            return producer;
        }

        #endregion
    }
}
