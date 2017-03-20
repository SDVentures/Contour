using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Common.Logging;
using Contour.Configuration;
using Contour.Helpers.CodeContracts;
using Contour.Receiving;
using Contour.Receiving.Consumers;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// The rabbit receiver.
    /// </summary>
    internal class RabbitReceiver : AbstractReceiver
    {
        private readonly ILog logger = LogManager.GetCurrentClassLogger();

        private readonly RabbitBus bus;

        private readonly IConnectionPool<IRabbitConnection> connectionPool;
        private readonly object syncRoot = new object();
        private readonly ConcurrentBag<Listener> listeners = new ConcurrentBag<Listener>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitReceiver"/> class. 
        /// </summary>
        /// <param name="bus">A reference to the bus containing the receiver</param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="connectionPool">
        /// A bus connection pool
        /// </param>
        public RabbitReceiver(RabbitBus bus, IReceiverConfiguration configuration, IConnectionPool<IRabbitConnection> connectionPool)
            : base(configuration)
        {
            this.bus = bus;
            this.connectionPool = connectionPool;
        }

        /// <summary>
        /// Gets a value indicating whether is started.
        /// </summary>
        public bool IsStarted { get; private set; }

        public override bool IsHealthy
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.listeners.Any(l => !l.HasFailed);
                }
            }
        }

        /// <summary>
        /// Checks if a receiver is able to process messages with <paramref name="label"/> label
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanReceive(MessageLabel label)
        {
            lock (this.syncRoot)
            {
                return this.listeners.Any(l => l.Supports(label));
            }
        }

        /// <summary>
        /// Registers a new consumer of messages with label <paramref name="label"/>
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="consumer">
        /// The consumer.
        /// </param>
        /// <typeparam name="T">
        /// The message payload type
        /// </typeparam>
        public override void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer)
        {
            this.logger.Trace(m => m("Registering consumer of [{0}] for receiver of [{1}].", typeof (T).Name, label));

            lock (this.syncRoot)
            {
                foreach (var listener in this.listeners)
                {
                    listener.RegisterConsumer(label, consumer, this.Configuration.Validator);
                }
            }
        }

        /// <summary>
        /// Starts the message receiver
        /// </summary>
        public override void Start()
        {
            if (this.IsStarted)
            {
                return;
            }

            this.logger.Trace(m => m("Starting receiver of [{0}].", this.Configuration.Label));

            this.StartListeners();
            this.IsStarted = true;
        }

        /// <summary>
        /// Stops the message receiver
        /// </summary>
        public override void Stop()
        {
            if (!this.IsStarted)
            {
                return;
            }

            this.logger.Trace(m => m("Stopping receiver of [{0}].", this.Configuration.Label));

            this.StopListeners();
            this.IsStarted = false;
        }

        public Listener GetListener(Func<Listener, bool> predicate)
        {
            lock (this.syncRoot)
            {
                this.Configure();
                return this.listeners.FirstOrDefault(predicate);
            }
        }

        /// <summary>
        /// Starts the listeners
        /// </summary>
        private void StartListeners()
        {
            this.logger.Trace(m => m("Starting listeners in receiver of [{0}]", this.Configuration.Label));

            lock (this.syncRoot)
            {
                this.Configure();

                foreach (var listener in this.listeners)
                {
                    listener.StartConsuming();
                    this.logger.Trace($"Listener [{listener}] started successfully");
                }
            }
        }

        private void Configure()
        {
            lock (this.syncRoot)
            {
                if (!this.listeners.IsEmpty)
                {
                    return;
                }

                this.BuildListeners();
                this.Configuration.ReceiverRegistration(this);
            }
        }

        /// <summary>
        /// Stops the listeners
        /// </summary>
        private void StopListeners()
        {
            this.logger.Trace(m => m("Stopping listeners in receiver of [{0}]", this.Configuration.Label));

            lock (this.syncRoot)
            {
                Listener listener;
                while (this.listeners.Any() && this.listeners.TryTake(out listener))
                {
                    try
                    {
                        listener.StopConsuming();
                        listener.Dispose();
                        this.logger.Trace($"Listener [{listener}] stopped successfully");
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(
                            $"Failed to stop a listener [{listener}] in receiver of [{this.Configuration.Label}] due to {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Builds a set of listeners constructing one listener for each URL in the connection string
        /// </summary>
        private void BuildListeners()
        {
            var options = (RabbitReceiverOptions)this.Configuration.Options;

            var reuseConnectionProperty = options.GetReuseConnection();
            var reuseConnection = reuseConnectionProperty.HasValue && reuseConnectionProperty.Value;

            var rabbitConnectionString = new RabbitConnectionString(options.GetConnectionString().Value);
            this.logger.Trace(
                $"Building listeners of [{this.Configuration.Label}]:\r\n{string.Join("\r\n", rabbitConnectionString.Select(url => $"URL\t=>\t{url}"))}");

            foreach (var url in rabbitConnectionString)
            {
                var source = new CancellationTokenSource();
                var connection = this.connectionPool.Get(url, reuseConnection, source.Token);
                this.logger.Trace($"Using connection [{connection.Id}] at URL='{url}' to resolve a listener");

                var topologyBuilder = new TopologyBuilder(connection.OpenChannel());
                var builder = new SubscriptionEndpointBuilder(this.bus.Endpoint, topologyBuilder, this.Configuration);

                var endpointBuilder = this.Configuration.Options.GetEndpointBuilder();

                Assumes.True(endpointBuilder != null, "EndpointBuilder is null for [{0}].", this.Configuration.Label);

                var endpoint = endpointBuilder.Value(builder);

                var listener =
                    this.listeners.FirstOrDefault(l => l.Endpoint.ListeningSource.Equals(endpoint.ListeningSource));
                if (listener == null)
                {
                    listener = new Listener(
                        this.bus,
                        connection,
                        endpoint,
                        options,
                        this.bus.Configuration.ValidatorRegistry);

                    this.listeners.Add(listener);
                }
                else
                {
                    this.EnsureConfigurationIsCompatible(listener);
                }
            }
        }

        private void EnsureConfigurationIsCompatible(Listener listener)
        {
            var existing = listener.ReceiverOptions;
            var other = (RabbitReceiverOptions)this.Configuration.Options;

            Action<Func<RabbitReceiverOptions, object>, string> compareAndThrow = (getOption, optionName) =>
            {
                if (getOption(existing) != getOption(other))
                {
                    throw new BusConfigurationException(
                        "Listener on [{0}] is not compatible with subscription of [{1}] due to option mismatch [{2}]."
                            .FormatEx(listener.Endpoint.ListeningSource, this.Configuration.Label, optionName));
                }
            };

            compareAndThrow(o => o.IsAcceptRequired(), "AcceptIsRequired");
            compareAndThrow(o => o.GetParallelismLevel(), "ParallelismLevel");
            compareAndThrow(o => o.GetFailedDeliveryStrategy(), "FailedDeliveryStrategy");
            compareAndThrow(o => o.GetQoS(), "QoS");
        }
    }
}
