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
        private readonly RabbitReceiverOptions receiverOptions;

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
            this.receiverOptions = (RabbitReceiverOptions)configuration.Options;
            this.logger = LogManager.GetLogger($"{this.GetType().FullName}(Endpoint=\"{this.bus.Endpoint}\")");
        }

        public event EventHandler<ListenerRegisteredEventArgs> ListenerRegistered = (sender, args) => { };

        /// <summary>
        /// Gets a value indicating whether is started.
        /// </summary>
        public bool IsStarted { get; private set; }
        
        public override bool IsHealthy => this.IsStarted;

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

                var listenerLabels = string.Join(",", listener.AcceptedLabels);
                this.logger.Trace($"Listener of labels ({listenerLabels}) has registered a consumer of label [{label}]");
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

        /// <summary>
        /// Registers a specified <paramref name="listener"/> in this <see cref="RabbitReceiver"/>
        /// </summary>
        /// <param name="listener">A listener to be registered</param>
        public void RegisterListener(Listener listener)
        {
            var listenerLabels = string.Join(",", listener.AcceptedLabels);
            var listenerAddress = listener.Endpoint.ListeningSource.Address;

            this.logger.Trace(
                $"Registering an external listener of labels ({listenerLabels}) at address [{listenerAddress}] in receiver of [{this.Configuration.Label}]");
            this.CheckIfCompatible(listener);

            if (!this.listeners.Contains(listener))
            {
                this.listeners.Add(listener);
                this.logger.Trace(
                    $"External listener of labels ({listenerLabels}) at address [{listenerAddress}] has been registered in receiver of [{this.Configuration.Label}]");
            }

            // Update consumer registrations in all listeners; this may possibly register more then one label-consumer pairs for each listener
            this.Configuration.ReceiverRegistration?.Invoke(this);
        }

        public Listener GetListener(Func<Listener, bool> predicate)
        {
            this.Configure();
            return this.listeners.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Checks if a <paramref name="listener"/> created by some other receiver is compatible with this one
        /// </summary>
        /// <param name="listener">A listener to check</param>
        /// <exception cref="BusConfigurationException">Raises a <see cref="BusConfigurationException"/> error if <paramref name="listener"/> is not compatible</exception>
        public void CheckIfCompatible(Listener listener)
        {
            var listenerOptions = listener.ReceiverOptions;

            // Check only listeners at the same URL and attached to the same listening source (queue); ensure the listener is not one of this receiver's listeners
            var checkList =
                this.listeners.Where(
                    l =>
                        l != listener
                        && l.BrokerUrl == listener.BrokerUrl
                        && l.Endpoint.ListeningSource.Equals(listener.Endpoint.ListeningSource));

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
            this.BuildListeners();

            // Update consumer registrations in all listeners
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
            var reuseConnectionProperty = this.receiverOptions.GetReuseConnection();
            var reuseConnection = reuseConnectionProperty.HasValue && reuseConnectionProperty.Value;
            
            this.logger.Trace(
                $"Building listeners of [{this.Configuration.Label}]:\r\n\t{string.Join("\r\n\t", this.receiverOptions.RabbitConnectionString.Select(url => $"Listener({this.Configuration.Label}): URL\t=>\t{url}"))}");

            foreach (var url in this.receiverOptions.RabbitConnectionString)
            {
                var source = new CancellationTokenSource();
                var connection = this.connectionPool.Get(url, reuseConnection, source.Token);
                this.logger.Trace($"Using connection [{connection.Id}] at URL=[{url}] to resolve a listener");

                var topologyBuilder = new TopologyBuilder(connection.OpenChannel());
                var builder = new SubscriptionEndpointBuilder(this.bus.Endpoint, topologyBuilder, this.Configuration);

                var endpointBuilder = this.Configuration.Options.GetEndpointBuilder();
                Assumes.True(endpointBuilder != null, "EndpointBuilder is null for [{0}].", this.Configuration.Label);

                var endpoint = endpointBuilder.Value(builder);

                var newListener = new Listener(
                    this.bus,
                    connection,
                    endpoint,
                    this.receiverOptions,
                    this.bus.Configuration.ValidatorRegistry);

                // There is no need to register another listener at the same URL and for the same source; consuming actions can be registered for a single listener
                var listener =
                    this.listeners.FirstOrDefault(
                        l =>
                            l.BrokerUrl == newListener.BrokerUrl &&
                            newListener.Endpoint.ListeningSource.Equals(l.Endpoint.ListeningSource));

                if (listener == null)
                {
                    listener = newListener;
                    this.listeners.Add(listener);
                }
                else
                {
                    // Check if an existing listener can be a substitute for a new one and if so just skip the new listeners
                    this.CheckIfCompatible(newListener);
                    listener = newListener;
                }

                // This event should be fired always, no matter if a listener already existed; this will ensure the event will be handled outside (in the bus)
                this.ListenerRegistered(this, new ListenerRegisteredEventArgs(listener));
            }
        }
    }
}
