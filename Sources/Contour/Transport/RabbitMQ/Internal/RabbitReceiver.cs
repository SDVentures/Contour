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
        private readonly ILog logger;
        private readonly RabbitBus bus;
        private readonly IConnectionPool<IRabbitConnection> connectionPool;
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
            this.logger = LogManager.GetLogger($"{this.GetType().Name}(Endpoint=\"{this.bus.Endpoint}\")");
        }

        public event EventHandler<ListenerRegisteredEventArgs> ListenerRegistered = (sender, args) => { };

        /// <summary>
        /// Gets a value indicating whether is started.
        /// </summary>
        public bool IsStarted { get; private set; }
        
        public override bool IsHealthy
        {
            get
            {
                return this.listeners.Any(l => !l.HasFailed);
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
            this.Configure();
            return this.listeners.Any(l => l.Supports(label));
        }

        /// <summary>
        /// Checks if a <paramref name="listener"/> is compatible with this receiver
        /// </summary>
        /// <param name="listener">A listener to check</param>
        /// <exception cref="BusConfigurationException">Raises a <see cref="BusConfigurationException"/> error if <paramref name="listener"/> is not compatible</exception>
        public void IsCompatible(Listener listener)
        {
            var listenerOptions = listener.ReceiverOptions;

            //// Check only listeners attached to the same listening source (queue)
            var checkList =
                this.listeners.Where(
                    l => l != listener && l.Endpoint.ListeningSource.Equals(listener.Endpoint.ListeningSource));
            
            foreach (var existingListener in checkList)
            {
                var existingOptions = existingListener.ReceiverOptions;

                Action<Func<RabbitReceiverOptions, object>, string> compareAndThrow = (getOption, optionName) =>
                {
                    if (getOption(existingOptions) != getOption(listenerOptions))
                    {
                        throw
                            new BusConfigurationException(
                                $"Listener on [{listener.Endpoint.ListeningSource}] is not compatible with subscription of [{this.Configuration.Label}] due to option mismatch [{optionName}]");
                    }
                };

                compareAndThrow(o => o.IsAcceptRequired(), "AcceptIsRequired");
                compareAndThrow(o => o.GetParallelismLevel(), "ParallelismLevel");
                compareAndThrow(o => o.GetFailedDeliveryStrategy(), "FailedDeliveryStrategy");
                compareAndThrow(o => o.GetQoS(), "QoS");
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
            this.logger.Trace($"Registering consumer of [{typeof(T).Name}] in receiver of label [{label}]");

            foreach (var listener in this.listeners)
            {
                listener.RegisterConsumer(label, consumer, this.Configuration.Validator);
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
            this.Configure();
            return this.listeners.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Starts the listeners
        /// </summary>
        private void StartListeners()
        {
            this.logger.Trace(m => m("Starting listeners in receiver of [{0}]", this.Configuration.Label));
            this.Configure();

            foreach (var listener in this.listeners)
            {
                listener.StartConsuming();
                this.logger.Trace($"Listener of [{this.Configuration.Label}] started successfully");
            }
        }

        private void Configure()
        {
            if (!this.listeners.IsEmpty)
            {
                return;
            }

            this.BuildListeners();
            this.Configuration.ReceiverRegistration?.Invoke(this);
        }

        /// <summary>
        /// Stops the listeners
        /// </summary>
        private void StopListeners()
        {
            this.logger.Trace(m => m("Stopping listeners in receiver of [{0}]", this.Configuration.Label));

            Listener listener;
            while (this.listeners.Any() && this.listeners.TryTake(out listener))
            {
                try
                {
                    listener.StopConsuming();
                    listener.Dispose();
                    this.logger.Trace("Listener stopped successfully");
                }
                catch (Exception ex)
                {
                    this.logger.Error(
                        $"Failed to stop a listener [{listener}] in receiver of [{this.Configuration.Label}] due to {ex.Message}");
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
                $"Building listeners of [{this.Configuration.Label}]:\r\n\t{string.Join("\r\n\t", rabbitConnectionString.Select(url => $"Listener({this.Configuration.Label}): URL\t=>\t{url}"))}");

            foreach (var url in rabbitConnectionString)
            {
                var source = new CancellationTokenSource();
                var connection = this.connectionPool.Get(url, reuseConnection, source.Token);
                this.logger.Trace($"Using connection [{connection.Id}] at URL=[{url}] to resolve a listener");

                var topologyBuilder = new TopologyBuilder(connection.OpenChannel());
                var builder = new SubscriptionEndpointBuilder(this.bus.Endpoint, topologyBuilder, this.Configuration);

                var endpointBuilder = this.Configuration.Options.GetEndpointBuilder();
                Assumes.True(endpointBuilder != null, "EndpointBuilder is null for [{0}].", this.Configuration.Label);

                var endpoint = endpointBuilder.Value(builder);

                var listener = new Listener(
                    this.bus,
                    connection,
                    endpoint,
                    options,
                    this.bus.Configuration.ValidatorRegistry);

                this.listeners.Add(listener);
                this.ListenerRegistered(this, new ListenerRegisteredEventArgs(listener));
            }
        }
    }
}
