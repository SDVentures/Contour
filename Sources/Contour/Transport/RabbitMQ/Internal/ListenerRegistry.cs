namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Contour.Configuration;
    using Contour.Helpers;
    using Contour.Helpers.CodeContracts;
    using Contour.Receiving;
    using Contour.Transport.RabbitMQ.Topology;

    /// <summary>
    /// The listener registry.
    /// </summary>
    internal class ListenerRegistry : IDisposable
    {
        #region Fields

        /// <summary>
        /// The _bus.
        /// </summary>
        private readonly RabbitBus bus;

        /// <summary>
        /// The _listeners.
        /// </summary>
        private readonly ConcurrentBag<Listener> listeners = new ConcurrentBag<Listener>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ListenerRegistry"/>.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        public ListenerRegistry(RabbitBus bus)
        {
            this.bus = bus;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The can consume.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanConsume(MessageLabel label)
        {
            return this.listeners.Any(l => l.AcceptedLabels.Contains(label));
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Reset();
        }

        /// <summary>
        /// The reset.
        /// </summary>
        public void Reset()
        {
            Listener listener;
            while (this.listeners.TryTake(out listener))
            {
                listener.Dispose();
            }
        }

        /// <summary>
        /// The resolve for.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="Listener"/>.
        /// </returns>
        public Listener ResolveFor(IReceiverConfiguration configuration)
        {
            using (RabbitChannel channel = this.bus.OpenChannel())
            {
                var topologyBuilder = new TopologyBuilder(channel);
                var builder = new SubscriptionEndpointBuilder(this.bus.Endpoint, topologyBuilder, configuration);

                Maybe<Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint>> endpointBuilder = configuration.Options.GetEndpointBuilder();

                Assumes.True(endpointBuilder != null, "EndpointBuilder is null for [{0}].", configuration.Label);

                ISubscriptionEndpoint endpoint = endpointBuilder.Value(builder);

                lock (this.listeners)
                {
                    Listener listener = this.listeners.FirstOrDefault(l => l.Endpoint.ListeningSource.Equals(endpoint.ListeningSource));
                    if (listener == null)
                    {
                        listener = new Listener(this.bus, endpoint, (RabbitReceiverOptions)configuration.Options, this.bus.Configuration.ValidatorRegistry);
                        this.listeners.Add(listener);
                    }
                    else
                    {
                        EnsureConfigurationIsCompatible(listener, configuration);
                    }

                    return listener;
                }
            }
        }

        /// <summary>
        /// The resolve for.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="Listener"/>.
        /// </returns>
        public Listener ResolveFor(MessageLabel label)
        {
            return this.listeners.First(l => l.Supports(label));
        }

        /// <summary>
        /// The start consuming.
        /// </summary>
        public void StartConsuming()
        {
                this.listeners.ForEach(l => l.StartConsuming());
        }

        /// <summary>
        /// The stop consuming.
        /// </summary>
        public void StopConsuming()
        {
                this.listeners.ForEach(l => l.StopConsuming());
        }

        #endregion

        #region Methods

        /// <summary>
        /// The ensure configuration is compatible.
        /// </summary>
        /// <param name="listener">
        /// The listener.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <exception cref="BusConfigurationException">
        /// </exception>
        private static void EnsureConfigurationIsCompatible(Listener listener, IReceiverConfiguration configuration)
        {
            RabbitReceiverOptions existing = listener.ReceiverOptions;
            var other = (RabbitReceiverOptions)configuration.Options;

            Action<Func<RabbitReceiverOptions, object>, string> compareAndThrow = (getOption, optionName) =>
                {
                    if (getOption(existing) != getOption(other))
                    {
                        throw new BusConfigurationException("Listener on [{0}] is not compatible with subscription of [{1}] due to option mismatch [{2}].".FormatEx(listener.Endpoint.ListeningSource, configuration.Label, optionName));
                    }
                };

            compareAndThrow(o => o.IsAcceptRequired(), "AcceptIsRequired");
            compareAndThrow(o => o.GetParallelismLevel(), "ParallelismLevel");
            compareAndThrow(o => o.GetFailedDeliveryStrategy(), "FailedDeliveryStrategy");
            compareAndThrow(o => o.GetQoS(), "QoS");
        }

        #endregion
    }
}
