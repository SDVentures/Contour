using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Configuration;
using Contour.Filters;
using Contour.Helpers.CodeContracts;
using Contour.Receiving;
using Contour.Sending;
using Contour.Transport.RabbitMq.Topology;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// Отправитель сообщений с помощью брокера <c>RabbitMQ</c>.
    /// </summary>
    internal class RabbitSender : AbstractSender
    {
        private const int AttemptsDefault = 1;
        private readonly TimeSpan timeoutDefault = TimeSpan.FromSeconds(30);
        
        private readonly ILog logger;
        private readonly RabbitBus bus;
        private readonly IConnectionPool<IRabbitConnection> connectionPool;
        private readonly ConcurrentQueue<IProducer> producers = new ConcurrentQueue<IProducer>();
        private readonly RabbitSenderOptions senderOptions;
        private IProducerSelector producerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitSender"/> class. 
        /// </summary>
        /// <param name="bus">
        /// A reference to the bus containing the sender
        /// </param>
        /// <param name="configuration">
        /// Конфигурация отправителя сообщений.
        /// </param>
        /// <param name="connectionPool">
        /// A bus connection pool
        /// </param>
        /// <param name="filters">
        /// Фильтры сообщений.
        /// </param>
        public RabbitSender(RabbitBus bus, ISenderConfiguration configuration, IConnectionPool<IRabbitConnection> connectionPool, IEnumerable<IMessageExchangeFilter> filters)
            : base(bus.Endpoint, configuration, filters)
        {
            this.bus = bus;
            this.connectionPool = connectionPool;
            this.senderOptions = (RabbitSenderOptions)this.Configuration.Options;

            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.bus.Endpoint}, {this.Configuration.Label})");
        }

        /// <summary>
        /// Если <c>true</c> - запущен, иначе <c>false</c>.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Если <c>true</c> - отправитель работает без сбоев, иначе <c>false</c>.
        /// </summary>
        public override bool IsHealthy => this.IsStarted;

        /// <summary>
        /// Освобождает занятые ресурсы. И останавливает отправителя.
        /// </summary>
        public override void Dispose()
        {
            this.logger.Trace(m => m("Disposing sender of [{0}]", this.Configuration.Label));
            this.Stop();
        }

        /// <summary>
        /// Запускает отправителя.
        /// </summary>
        public override void Start()
        {
            if (this.IsStarted)
            {
                return;
            }

            this.logger.Trace(m => m("Starting sender of [{0}]", this.Configuration.Label));

            this.StartProducers();
            this.IsStarted = true;
        }

        /// <summary>
        /// Останавливает отправителя.
        /// </summary>
        public override void Stop()
        {
            if (!this.IsStarted)
            {
                return;
            }

            this.logger.Trace(m => m("Stopping sender of [{0}]", this.Configuration.Label));

            this.StopProducers();
            this.IsStarted = false;
        }

        /// <summary>
        /// Выполняет отправку сообщения.
        /// </summary>
        /// <param name="exchange">Информация об отправке.</param>
        /// <returns>Задача ожидания отправки сообщения.</returns>
        protected override Task<MessageExchange> InternalSend(MessageExchange exchange)
        {
            var attempts = this.senderOptions.GetFailoverAttempts() ?? AttemptsDefault;
            
            var wrapper = new FaultTolerantProducer(this.producerSelector, attempts);
            return wrapper.Try(exchange);
        }

        /// <summary>
        /// Starts the producers
        /// </summary>
        private void StartProducers()
        {
            this.logger.Trace(m => m("Starting producers for sender of [{0}]", this.Configuration.Label));
            this.Configure();

            foreach (var producer in this.producers)
            {
                producer.Start();
                this.logger.Trace($"Producer of [{this.Configuration.Label}] started successfully");
            }
        }

        private void Configure()
        {
            this.BuildProducers();
            var builder = this.senderOptions.GetProducerSelectorBuilder();
            this.producerSelector = builder.Build(this.producers);
        }

        /// <summary>
        /// Stops the producers
        /// </summary>
        private void StopProducers()
        {
            this.logger.Trace(m => m("Stopping producers for sender of [{0}]", this.Configuration.Label));

            IProducer producer;
            while (!this.producers.IsEmpty && this.producers.TryDequeue(out producer))
            {
                try
                {
                    producer.Stop();
                    producer.Dispose();
                    this.logger.Trace("Producer stopped successfully");
                }
                catch (Exception ex)
                {
                    this.logger.Error(
                        $"Failed to stop producer [{producer}] in sender of [{this.Configuration.Label}] due to {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Builds a set of producers constructing one producer for each URL in the connection string
        /// </summary>
        private void BuildProducers()
        {
            this.logger.Trace(
                $"Building producers of [{this.Configuration.Label}]:\r\n\t{string.Join("\r\n\t", this.senderOptions.RabbitConnectionString.Select(url => $"Producer({this.Configuration.Label}): URL\t=>\t{url}"))}");

            foreach (var url in this.senderOptions.RabbitConnectionString)
            {
                this.EnlistProducer(url);
            }
        }

        private IProducer EnlistProducer(string url)
        {
            this.logger.Trace($">>> Enlisting a new producer of [{this.Configuration.Label}] at URL=[{url}]...");
            var producer = this.BuildProducer(url);

            this.producers.Enqueue(producer);
            this.logger.Trace($"<<< A producer of [{producer.Label}] at URL=[{producer.BrokerUrl}] has been enlisted");

            return producer;
        }

        private Producer BuildProducer(string url)
        {
            var reuseConnectionProperty = this.senderOptions.GetReuseConnection();
            var reuseConnection = reuseConnectionProperty.HasValue && reuseConnectionProperty.Value;

            var source = new CancellationTokenSource();
            var connection = this.connectionPool.Get(url, reuseConnection, source.Token);
            this.logger.Trace($"Using connection [{connection.Id}] at URL=[{url}] to resolve a producer");

            using (var topologyBuilder = new TopologyBuilder(connection))
            {
                var builder = new RouteResolverBuilder(this.bus.Endpoint, topologyBuilder, this.Configuration);
                var routeResolverBuilderFunc = this.Configuration.Options.GetRouteResolverBuilder();

                Assumes.True(
                    routeResolverBuilderFunc.HasValue,
                    "RouteResolverBuilder must be set for [{0}]",
                    this.Configuration.Label);

                var routeResolver = routeResolverBuilderFunc.Value(builder);

                var producer = new Producer(
                    this.bus.Endpoint,
                    connection,
                    this.Configuration.Label,
                    routeResolver,
                    this.Configuration.Options.IsConfirmationRequired());

                if (this.Configuration.RequiresCallback)
                {
                    var callbackConfiguration = this.CreateCallbackReceiverConfiguration(url);
                    var receiver = this.bus.RegisterReceiver(callbackConfiguration);

                    this.logger.Trace(
                        $"A sender of [{this.Configuration.Label}] requires a callback configuration; registering a receiver of [{callbackConfiguration.Label}] with connection string [{callbackConfiguration.Options.GetConnectionString()}]");

                    this.logger.Trace(
                        $"A new callback receiver of [{callbackConfiguration.Label}] with connection string [{callbackConfiguration.Options.GetConnectionString()}] has been successfully registered, getting one of its listeners with URL=[{producer.BrokerUrl}]...");

                    var listener = receiver.GetListener(l => l.BrokerUrl == producer.BrokerUrl);

                    if (listener == null)
                    {
                        throw new BusConfigurationException(
                            $"Unable to find a suitable listener for receiver {receiver}");
                    }

                    this.logger.Trace(
                        $"A listener at URL=[{listener.BrokerUrl}] belonging to callback receiver of [{callbackConfiguration.Label}] acquired");
                    
                    listener.StopOnChannelShutdown = true;
                    producer.UseCallbackListener(listener);

                    producer.StopOnChannelShutdown = true;
                    producer.Stopped += (sender, args) =>
                    {
                        this.OnProducerStopped(url, sender, args);
                    };

                    this.logger.Trace(
                        $"A producer of [{producer.Label}] at URL=[{producer.BrokerUrl}] has registered a callback listener successfully");
                }

                return producer;
            }
        }

        private void OnProducerStopped(string url, object sender, ProducerStoppedEventArgs args)
        {
            if (args.Reason == OperationStopReason.Regular)
            {
                return;
            }

            this.logger.Warn($"Producer [{sender.GetHashCode()}] with response callback has been stopped and will be reenlisted");

            while (true)
            {
                IProducer delistedProducer;
                if (this.producers.TryDequeue(out delistedProducer))
                {
                    if (sender == delistedProducer)
                    {
                        this.logger.Trace($"Producer [{delistedProducer.GetHashCode()}] has been delisted");
                        break;
                    }

                    this.producers.Enqueue(delistedProducer);
                }
            }

            var newProducer = this.EnlistProducer(url);
            newProducer.Start();
        }

        /// <summary>
        /// Overrides the callback configuration connection string with <paramref name="url"/>
        /// since the callback configuration may contain a list of connection strings for sharding support.
        /// </summary>
        /// <param name="url">The connection string URL</param>
        /// <returns>The reply receiver configuration</returns>
        private ReceiverConfiguration CreateCallbackReceiverConfiguration(string url)
        {
            var callbackConfiguration = new ReceiverConfiguration(
                this.Configuration.Label,
                this.Configuration.CallbackConfiguration.Options);

            callbackConfiguration.WithConnectionString(url);
            return callbackConfiguration;
        }
    }
}
