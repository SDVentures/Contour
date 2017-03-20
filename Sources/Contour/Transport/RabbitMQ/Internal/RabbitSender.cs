using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Contour.Configuration;
using Contour.Filters;
using Contour.Helpers.CodeContracts;
using Contour.Sending;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Отправитель сообщений с помощью брокера <c>RabbitMQ</c>.
    /// </summary>
    internal class RabbitSender : AbstractSender
    {
        private readonly ILog logger = LogManager.GetCurrentClassLogger();
        private readonly RabbitBus bus;
        private readonly IConnectionPool<IRabbitConnection> connectionPool;
        private readonly object syncRoot = new object();
        private readonly IList<Producer> producers = new List<Producer>();
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
        }

        /// <summary>
        /// Если <c>true</c> - запущен, иначе <c>false</c>.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Если <c>true</c> - отправитель работает без сбоев, иначе <c>false</c>.
        /// </summary>
        public override bool IsHealthy
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.producers.Any(p => !p.HasFailed);
                }
            }
        }

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
            lock (this.syncRoot)
            {
                var producerIndex = this.producerSelector.Next();
                var producer = this.producers[producerIndex];

                if (exchange.IsRequest)
                {
                    return producer.Request(exchange.Out, exchange.ExpectedResponseType)
                        .ContinueWith(
                            t =>
                            {
                                if (t.IsFaulted)
                                {
                                    exchange.Exception = t.Exception;
                                }
                                else
                                {
                                    exchange.In = t.Result;
                                }

                                return exchange;
                            });
                }

                return producer.Publish(exchange.Out)
                    .ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted)
                            {
                                exchange.Exception = t.Exception;
                            }

                            return exchange;
                        });
            }
        }

        /// <summary>
        /// Starts the producers
        /// </summary>
        private void StartProducers()
        {
            this.logger.Trace(m => m("Starting producers for sender of [{0}]", this.Configuration.Label));

            this.BuildProducers();

            lock (this.syncRoot)
            {
                foreach (var producer in this.producers)
                {
                    producer.Start();
                    this.logger.Trace($"Producer [{producer}] started successfully");
                }
            }
        }

        /// <summary>
        /// Stops the producers
        /// </summary>
        private void StopProducers()
        {
            this.logger.Trace(m => m("Stopping producers for sender of [{0}]", this.Configuration.Label));

            lock (this.syncRoot)
            {
                while (this.producers.Any())
                {
                    var producer = this.producers.First();
                    try
                    {
                        producer.Stop();
                        producer.Dispose();

                        this.producers.Remove(producer);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(
                            $"Failed to stop producer [{producer}] in sender of [{this.Configuration.Label}] due to {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Builds a set of producers constructing one producer for each URL in the connection string
        /// </summary>
        private void BuildProducers()
        {
            var options = (RabbitSenderOptions)this.Configuration.Options;
            this.producerSelector = options.GetProducerSelector();

            var reuseConnectionProperty = options.GetReuseConnection();
            var reuseConnection = reuseConnectionProperty.HasValue && reuseConnectionProperty.Value;

            var rabbitConnectionString = new RabbitConnectionString(options.GetConnectionString().Value);

            this.logger.Trace(
                $"Building producers of [{this.Configuration.Label}]:\r\n{string.Join("\r\n", rabbitConnectionString.Select(url => $"URL\t=>\t{url}"))}");

            lock (this.syncRoot)
            {
                foreach (var url in rabbitConnectionString)
                {
                    var source = new CancellationTokenSource();
                    var connection = this.connectionPool.Get(url, reuseConnection, source.Token);
                    this.logger.Trace($"Using connection [{connection.Id}] at URL='{url}' to resolve a producer");

                    var topologyBuilder = new TopologyBuilder(connection.OpenChannel());
                    var builder = new RouteResolverBuilder(this.bus.Endpoint, topologyBuilder, this.Configuration);
                    var routeResolverBuilder = this.Configuration.Options.GetRouteResolverBuilder();

                    Assumes.True(
                        routeResolverBuilder.HasValue,
                        "RouteResolverBuilder must be set for [{0}]",
                        this.Configuration.Label);

                    var routeResolver = routeResolverBuilder.Value(builder);

                    var producer = new Producer(
                        this.bus.Endpoint,
                        connection,
                        this.Configuration.Label,
                        routeResolver,
                        this.Configuration.Options.IsConfirmationRequired());

                    producer.Failed += p =>
                    {
                        // A failed producer will not be removed from the collection cause this failure should be treated as transient
                        this.logger.Error($"Producer [{producer}] has failed");
                    };

                    if (this.Configuration.RequiresCallback)
                    {
                        var callbackConfiguration = this.Configuration.CallbackConfiguration;

                        this.logger.Trace(
                            $"A sender of [{this.Configuration.Label}] requires a callback configuration, registering a receiver of [{callbackConfiguration.Label}] with connection string [{callbackConfiguration.Options.GetConnectionString()}]");

                        var receiver = this.bus.RegisterReceiver(this.Configuration.CallbackConfiguration);

                        this.logger.Trace($"A new callback receiver of [{callbackConfiguration.Label}] with connection string [{callbackConfiguration.Options.GetConnectionString()}] has been successfully registered, getting a listener...");
                        
                        var listener =
                            receiver.GetListener(
                                l =>
                                    this.Configuration.Options.GetConnectionString() ==
                                    l.ReceiverOptions.GetConnectionString());
                        
                        if (listener == null)
                        {
                            throw new BusConfigurationException(
                                $"Unable to find a suitable listener for receiver {receiver}");
                        }

                        this.logger.Trace($"A listener at URL='{listener.BrokerUrl}' belonging to callback receiver of [{callbackConfiguration.Label}] with connection string [{callbackConfiguration.Options.GetConnectionString()}] acquired");

                        producer.UseCallbackListener(listener);

                        this.logger.Trace($"A producer of [{producer.Label}] at URL='{producer.BrokerUrl}' has registered a callback listener successfully");
                    }

                    this.producers.Add(producer);
                    this.logger.Trace($"A producer of [{producer.Label}] at URL='{producer.BrokerUrl}' has been added to the sender");
                }

                this.producerSelector.Initialize((IList)this.producers);
            }
        }
    }
}
