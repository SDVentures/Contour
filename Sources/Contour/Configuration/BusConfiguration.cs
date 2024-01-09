using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Contour.Configurator;
using Contour.Filters;
using Contour.Helpers;
using Contour.Receiving;
using Contour.Sending;
using Contour.Serialization;
using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Internal;
using Contour.Validation;
 
namespace Contour.Configuration
{
    /// <summary>
    /// The bus configuration.
    /// </summary>
    internal class BusConfiguration : IBusConfigurator, IBusConfiguration
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<BusConfiguration>();

        /// <summary>
        /// The _filters.
        /// </summary>
        private readonly IList<IMessageExchangeFilter> filters = new List<IMessageExchangeFilter>();

        /// <summary>
        /// The _message label resolver.
        /// </summary>
        private readonly MessageLabelResolver messageLabelResolver = new MessageLabelResolver();

        /// <summary>
        /// The _receiver configurations.
        /// </summary>
        private readonly IList<IReceiverConfiguration> receiverConfigurations = new List<IReceiverConfiguration>();

        /// <summary>
        /// The _sender configurations.
        /// </summary>
        private readonly IList<ISenderConfiguration> senderConfigurations = new List<ISenderConfiguration>();

        /// <summary>
        /// The _validator registry.
        /// </summary>
        private readonly MessageValidatorRegistry validatorRegistry = new MessageValidatorRegistry();

        /// <summary>
        /// Initializes a new instance of the <see cref="BusConfiguration"/> class. 
        /// </summary>
        public BusConfiguration()
        {
            Logger.Trace(m => m("Вызван конструктор BusConfiguration"));

            this.EndpointOptions = new EndpointOptions();

            this.SenderDefaults = new SenderOptions(this.EndpointOptions);
            this.ReceiverDefaults = new ReceiverOptions(this.EndpointOptions);
        }

        /// <summary>
        /// Gets the bus factory func.
        /// </summary>
        public Func<IBusConfigurator, IBus> BusFactoryFunc { get; private set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        [Obsolete("Use EndpointOptions instead")]
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the default subscription endpoint builder.
        /// </summary>
        public Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> DefaultSubscriptionEndpointBuilder { get; private set; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public IEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        public IEnumerable<IMessageExchangeFilter> Filters
        {
            get
            {
                return this.filters;
            }
        }

        /// <summary>
        /// Gets the lifecycle handler.
        /// </summary>
        public IBusLifecycleHandler LifecycleHandler { get; private set; }

        /// <summary>
        /// Gets the message label handler.
        /// </summary>
        public IMessageLabelHandler MessageLabelHandler { get; private set; }

        /// <summary>
        /// Gets the message label resolver.
        /// </summary>
        public MessageLabelResolver MessageLabelResolver
        {
            get
            {
                return this.messageLabelResolver;
            }
        }

        /// <summary>
        /// Gets the endpoint options.
        /// </summary>
        public EndpointOptions EndpointOptions { get; internal set; }

        /// <summary>
        /// Gets the receiver configurations.
        /// </summary>
        public IEnumerable<IReceiverConfiguration> ReceiverConfigurations
        {
            get
            {
                return this.receiverConfigurations;
            }
        }

        /// <summary>
        /// Gets the receiver defaults.
        /// </summary>
        public ReceiverOptions ReceiverDefaults { get; internal set; }

        /// <summary>
        /// Gets the sender configurations.
        /// </summary>
        public IEnumerable<ISenderConfiguration> SenderConfigurations
        {
            get
            {
                return this.senderConfigurations;
            }
        }

        /// <summary>
        /// Gets the sender defaults.
        /// </summary>
        public SenderOptions SenderDefaults { get; internal set; }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IPayloadConverter Serializer { get; private set; }
        
        /// <summary>
        /// Gets the validator registry.
        /// </summary>
        public MessageValidatorRegistry ValidatorRegistry
        {
            get
            {
                return this.validatorRegistry;
            }
        }

        /// <summary>
        /// The build bus using.
        /// </summary>
        /// <param name="busFactoryFunc">
        /// The bus factory func.
        /// </param>
        /// <returns>
        /// The <see cref="IBusConfigurator"/>.
        /// </returns>
        public IBusConfigurator BuildBusUsing(Func<IBusConfigurator, IBus> busFactoryFunc)
        {
            this.BusFactoryFunc = busFactoryFunc;

            return this;
        }

        /// <summary>
        /// The handle lifecycle with.
        /// </summary>
        /// <param name="lifecycleHandler">
        /// The lifecycle handler.
        /// </param>
        public void HandleLifecycleWith(IBusLifecycleHandler lifecycleHandler)
        {
            this.LifecycleHandler = lifecycleHandler;
        }

        /// <summary>
        /// The on.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public IReceiverConfigurator<T> On<T>(string label) where T : class
        {
            return On(label).As<T>();
        }

        /// <summary>
        /// The on.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public IReceiverConfigurator<T> On<T>(MessageLabel label) where T : class
        {
            return this.On(label).As<T>();
        }

        /// <summary>
        /// The on.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public IReceiverConfigurator<T> On<T>() where T : class
        {
            return this.On<T>(this.MessageLabelResolver.ResolveFrom<T>());
        }

        /// <summary>
        /// The on.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public IReceiverConfigurator On(string label)
        {
            return this.On(MessageLabel.From(label));
        }

        /// <summary>
        /// The on.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Raises an error if a receiver for <paramref name="label"/> has already been registered
        /// </exception>
        public IReceiverConfigurator On(MessageLabel label)
        {
            if (this.HasRegisteredConsumerFor(label))
            {
                throw new ArgumentException($"Receiver for label [{label}] is already registered", nameof(label));
            }

            var provider = this.ReceiverDefaults.GetConnectionStringProvider();
            var connectionString = provider?.GetConnectionString(label);

            if (!string.IsNullOrEmpty(connectionString))
            {
                this.ReceiverDefaults.ConnectionString = connectionString;
            }

            var configuration = new ReceiverConfiguration(label, this.ReceiverDefaults);
            this.receiverConfigurations.Add(configuration);

            return configuration;
        }

        /// <summary>
        /// The on failed.
        /// </summary>
        /// <param name="failedDeliveryStrategy">
        /// The failed delivery strategy.
        /// </param>
        public void OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy)
        {
            this.ReceiverDefaults.FailedDeliveryStrategy = failedDeliveryStrategy.Maybe();
        }

        /// <summary>
        /// The on failed.
        /// </summary>
        /// <param name="failedDeliveryHandler">
        /// The failed delivery handler.
        /// </param>
        public void OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler)
        {
            this.OnFailed(new LambdaFailedDeliveryStrategy(failedDeliveryHandler));
        }

        /// <summary>
        /// The on unhandled.
        /// </summary>
        /// <param name="unhandledDeliveryStrategy">
        /// The unhandled delivery strategy.
        /// </param>
        public void OnUnhandled(IUnhandledDeliveryStrategy unhandledDeliveryStrategy)
        {
            this.ReceiverDefaults.UnhandledDeliveryStrategy = unhandledDeliveryStrategy.Maybe();
        }

        /// <summary>
        /// The on unhandled.
        /// </summary>
        /// <param name="unhandledDeliveryHandler">
        /// The unhandled delivery handler.
        /// </param>
        public void OnUnhandled(Action<IFaultedConsumingContext> unhandledDeliveryHandler)
        {
            this.OnUnhandled(new LambdaUnhandledDeliveryStrategy(unhandledDeliveryHandler));
        }

        /// <summary>
        /// The register filter.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        public void RegisterFilter(IMessageExchangeFilter filter)
        {
            this.filters.Add(filter);
        }

        /// <summary>
        /// The register validator.
        /// </summary>
        /// <param name="validator">
        /// The validator.
        /// </param>
        public void RegisterValidator(IMessageValidator validator)
        {
            this.ValidatorRegistry.Register(validator);
        }

        /// <summary>
        /// The register validators.
        /// </summary>
        /// <param name="validatorGroup">
        /// The validator group.
        /// </param>
        public void RegisterValidators(MessageValidatorGroup validatorGroup)
        {
            this.ValidatorRegistry.Register(validatorGroup);
        }

        /// <summary>
        /// The route.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="ISenderConfigurator"/>.
        /// </returns>
        public ISenderConfigurator Route(string label)
        {
            return this.Route(label.ToMessageLabel());
        }

        /// <summary>
        /// The route.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="ISenderConfigurator"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Raises an error if a sender for <paramref name="label"/> has already been registered
        /// </exception>
        public ISenderConfigurator Route(MessageLabel label)
        {
            if (this.HasRegisteredProducerFor(label))
            {
                throw new ArgumentException($"Sender for label [{label}] already registered.", nameof(label));
            }

            var provider = this.SenderDefaults.GetConnectionStringProvider();
            var connectionString = provider?.GetConnectionString(label);

            if (!string.IsNullOrEmpty(connectionString))
            {
                this.SenderDefaults.ConnectionString = connectionString;
            }

            var configuration = new SenderConfiguration(label, this.SenderDefaults, this.ReceiverDefaults);
            this.senderConfigurations.Add(configuration);
            return configuration;
        }

        /// <summary>
        /// The route.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ISenderConfigurator"/>.
        /// </returns>
        public ISenderConfigurator Route<T>() where T : class
        {
            return this.Route(this.MessageLabelResolver.ResolveFrom<T>());
        }

        /// <summary>
        /// The set connection string.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public void SetConnectionString(string connectionString)
        {
            Logger.Trace(m => m("Установка строки подключения к брокеру RabbitMQ [{0}].", connectionString));

            this.ConnectionString = connectionString;
            this.EndpointOptions.ConnectionString = connectionString;

            Logger.Debug(m => m("Установлена строка подключения [{0}].", this.ConnectionString));
        }

        /// <summary>
        /// Specifies if a connection can be reused.
        /// </summary>
        /// <param name="reuse">
        /// The reuse.
        /// </param>
        public void ReuseConnection(bool reuse = true)
        {
            Logger.Trace("Setting connection reuse");
            this.EndpointOptions.ReuseConnection = reuse;
            Logger.Debug("Connection reuse is set");
        }

        /// <summary>
        /// The set endpoint.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        public void SetEndpoint(string address)
        {
            Logger.Trace(m => m("Установка адреса конечной точки [{0}].", address));

            this.Endpoint = new Endpoint(address);

            Logger.Debug(m => m("Установлен адрес конечной точки [{0}].", this.Endpoint));
        }

        /// <summary>
        /// The use message label handler.
        /// </summary>
        /// <param name="messageLabelHandler">
        /// The message label handler.
        /// </param>
        public void UseMessageLabelHandler(IMessageLabelHandler messageLabelHandler)
        {
            this.MessageLabelHandler = messageLabelHandler;
        }

        /// <summary>
        /// The use parallelism level.
        /// </summary>
        /// <param name="parallelismLevel">
        /// The parallelism level.
        /// </param>
        public void UseParallelismLevel(uint parallelismLevel)
        {
            this.ReceiverDefaults.ParallelismLevel = parallelismLevel;
        }

        /// <summary>
        /// The fault queue message TTL.
        /// </summary>
        /// <param name="messageTtl">the fault queue message TTL.</param>
        public void UseFaultQueueTtl(TimeSpan messageTtl)
        {
            this.ReceiverDefaults.FaultQueueTtl = messageTtl;
        }

        /// <summary>
        /// The fault message queue length limit.
        /// </summary>
        /// <param name="queueLimit">The fault message queue length limit.</param>
        public void UseFaultQueueLimit(int queueLimit)
        {
            this.ReceiverDefaults.FaultQueueLimit = queueLimit;
        }

        /// <summary>
        /// The message queue length limit.
        /// </summary>
        /// <param name="queueLimit">The message queue length limit.</param>
        public void UseQueueLimit(int queueLimit)
        {
            this.ReceiverDefaults.QueueLimit = queueLimit;
        }

        /// <summary>
        /// The message queue length limit in bytes.
        /// </summary>
        /// <param name="bytes">The message queue limit in bytes.</param>
        public void UseQueueMaxLengthBytes(int bytes)
        {
            this.ReceiverDefaults.QueueMaxLengthBytes = bytes;
        }

        /// <summary>
        /// The use payload converter.
        /// </summary>
        /// <param name="converter">
        /// The converter.
        /// </param>
        public void UsePayloadConverter(IPayloadConverter converter)
        {
            this.Serializer = converter;
        }

        /// <summary>
        /// The use request timeout.
        /// </summary>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        public void UseRequestTimeout(TimeSpan? timeout)
        {
            this.SenderDefaults.RequestTimeout = timeout;
        }

        /// <summary>
        /// The use route resolver builder.
        /// </summary>
        /// <param name="routeResolverBuilder">
        /// The route resolver builder.
        /// </param>
        public void UseRouteResolverBuilder(Func<IRouteResolverBuilder, IRouteResolver> routeResolverBuilder)
        {
            this.SenderDefaults.RouteResolverBuilder = routeResolverBuilder;
        }

        /// <summary>
        /// The use subscription endpoint builder.
        /// </summary>
        /// <param name="endpointBuilder">
        /// The endpoint builder.
        /// </param>
        public void UseSubscriptionEndpointBuilder(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder)
        {
            this.DefaultSubscriptionEndpointBuilder = endpointBuilder;
        }

        /// <inheritdoc />
        public void SetExcludedIncomingHeaders(IEnumerable<string> excludedHeaders)
        {
            var headers = excludedHeaders ?? Enumerable.Empty<string>();
            var storage = this.EndpointOptions.GetIncomingMessageHeaderStorage();
            if (storage.HasValue)
            {
                storage.Value.RegisterExcludedHeaders(headers);
            }
        }

        public void UseIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage)
        {
            this.EndpointOptions.IncomingMessageHeaderStorage = new Maybe<IIncomingMessageHeaderStorage>(storage);
        }

        public void UseConnectionStringProvider(IConnectionStringProvider provider)
        {
            this.EndpointOptions.ConnectionStringProvider = provider;
        }
        
        public void SetExperimentalProducerSelector()
        {
            if (SenderDefaults is RabbitSenderOptions rabbitSenderOptions)
            {
                rabbitSenderOptions.ProducerSelectorBuilder = new ExperimentalProducerSelectorBuilder();
            }
            else
            {
                Logger.Warn("SetExperimentalProducerSelector called, but not implemented");
            }
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <exception cref="BusConfigurationException">
        /// </exception>
        public void Validate()
        {
            Logger.Trace(m => m("Вызван метод для валидации конфигурации. Строка соединия - [{0}], адрес конечной точки - [{1}], получаемые сообщения - [{2}], отправляемые сообщения - [{3}]", this.ConnectionString, this.Endpoint, this.ReceiverConfigurations != null ? string.Join(";", this.ReceiverConfigurations.Select(x => x.Label)) : "null", this.SenderConfigurations != null ? string.Join(";", this.SenderConfigurations.Select(x => x.Label)) : "null"));

            if (this.Serializer == null)
            {
                throw new BusConfigurationException("PayloadConverter is not set.");
            }

            if (this.BusFactoryFunc == null)
            {
                throw new BusConfigurationException("Bus factory is not set.");
            }

            if (this.Endpoint == null)
            {
                throw new BusConfigurationException("Не задано название компонента (Endpoint).");
            }

            if (!this.ReceiverConfigurations.Any() && !this.SenderConfigurations.Any())
            {
                throw new BusConfigurationException("No senders and receivers are registered.");
            }

            foreach (ISenderConfiguration producer in this.SenderConfigurations)
            {
                producer.Validate();
            }

            foreach (IReceiverConfiguration consumer in this.ReceiverConfigurations)
            {
                consumer.Validate();
            }

            Logger.Trace(m => m("Конец метода валидации конфигурации"));
        }

        /// <summary>
        /// The has registered consumer for.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasRegisteredConsumerFor(MessageLabel label)
        {
            return this.ReceiverConfigurations.Any(c => c.Label.Equals(label));
        }

        /// <summary>
        /// The has registered producer for.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasRegisteredProducerFor(MessageLabel label)
        {
            return this.SenderConfigurations.Any(c => c.Label.Equals(label));
        }
    }
}
