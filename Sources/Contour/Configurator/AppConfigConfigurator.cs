using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Contour.Configuration;
using Contour.Configurator.Configuration;
using Contour.Receiving;
using Contour.Receiving.Consumers;
using Contour.Sending;
using Contour.Validation;

using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Internal;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Configurator
{
    /// <summary>
    ///   Конфигуратор клиента шины сообщений, использующий настройки объявленные в стандартном .config-файле.
    /// </summary>
    public class AppConfigConfigurator : IConfigurator
    {
        /// <summary>
        /// The service bus section name.
        /// </summary>
        private const string ServiceBusSectionName = "serviceBus/endpoints";

        /// <summary>
        /// The _dependency resolver.
        /// </summary>
        private readonly IDependencyResolver dependencyResolver;

        /// <summary>
        /// The _endpoints config.
        /// </summary>
        private readonly IDictionary<string, Configuration.IEndpoint> endpointsConfig;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AppConfigConfigurator"/>.
        /// </summary>
        /// <param name="dependencyResolver">
        /// The dependency resolver.
        /// </param>
        public AppConfigConfigurator(IDependencyResolver dependencyResolver)
            : this((EndpointsSection)ConfigurationManager.GetSection(ServiceBusSectionName), dependencyResolver)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AppConfigConfigurator"/>.
        /// </summary>
        /// <param name="dependencyResolverFunc">
        /// The dependency resolver func.
        /// </param>
        public AppConfigConfigurator(DependencyResolverFunc dependencyResolverFunc)
            : this((EndpointsSection)ConfigurationManager.GetSection(ServiceBusSectionName), new LambdaDependencyResolver(dependencyResolverFunc))
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AppConfigConfigurator"/>.
        /// </summary>
        public AppConfigConfigurator()
            : this((EndpointsSection)ConfigurationManager.GetSection(ServiceBusSectionName), new StubDependencyResolver())
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AppConfigConfigurator"/>.
        /// </summary>
        /// <param name="endpointsConfig">
        /// The endpoints config.
        /// </param>
        /// <param name="dependencyResolverFunc">
        /// The dependency resolver func.
        /// </param>
        public AppConfigConfigurator(IBusConfigurationSection endpointsConfig, DependencyResolverFunc dependencyResolverFunc)
            : this(endpointsConfig, new LambdaDependencyResolver(dependencyResolverFunc))
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AppConfigConfigurator"/>.
        /// </summary>
        /// <param name="endpointsConfig">
        /// The endpoints config.
        /// </param>
        /// <param name="dependencyResolver">
        /// The dependency resolver.
        /// </param>
        internal AppConfigConfigurator(IBusConfigurationSection endpointsConfig, IDependencyResolver dependencyResolver)
        {
            this.endpointsConfig = endpointsConfig.Endpoints.ToDictionary(e => e.Name);
            this.dependencyResolver = dependencyResolver;
        }

        /// <summary>
        ///   Имена точек подключения к шине.
        /// </summary>
        public IEnumerable<string> Endpoints => this.endpointsConfig.Keys;

        /// <summary>
        /// Конфигурирует клиента шины сообщений.
        /// </summary>
        /// <param name="endpointName">
        /// Имя точки подключения к шине.
        /// </param>
        /// <param name="cfg">
        /// Конфигуратор клиента шины.
        /// </param>
        /// <returns>
        /// Конфигуратор клиента шины, после применения к нему всех настроек.
        /// </returns>
        public IBusConfigurator Configure(string endpointName, IBusConfigurator cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg", "Файл конфигурации не может быть null");
            }

            var endpointConfig = this.GetEndPointByName(endpointName);

            IConnectionStringProvider connectionStringProvider = null;
            if (!string.IsNullOrEmpty(endpointConfig.ConnectionStringProvider))
            {
                connectionStringProvider = this.GetConnectionStringProvider(endpointConfig.ConnectionStringProvider);
            }

            cfg.SetEndpoint(endpointConfig.Name);
            cfg.SetConnectionString(endpointConfig.ConnectionString);
            cfg.SetExcludedIncomingHeaders(endpointConfig.ExcludedHeaders);

            if (endpointConfig.ReuseConnection.HasValue)
            {
                cfg.ReuseConnection(endpointConfig.ReuseConnection.Value);
            }

            if (!string.IsNullOrWhiteSpace(endpointConfig.LifecycleHandler))
            {
                cfg.HandleLifecycleWith(this.ResolveLifecycleHandler(endpointConfig.LifecycleHandler));
            }

            if (endpointConfig.ParallelismLevel.HasValue)
            {
                cfg.UseParallelismLevel(endpointConfig.ParallelismLevel.Value);
            }

            if (endpointConfig.FaultQueueTtl.HasValue)
            {
                cfg.UseFaultQueueTtl(endpointConfig.FaultQueueTtl.Value);
            }

            if (endpointConfig.FaultQueueLimit.HasValue)
            {
                cfg.UseFaultQueueLimit(endpointConfig.FaultQueueLimit.Value);
            }

            if (endpointConfig.QueueLimit.HasValue)
            {
                cfg.UseQueueLimit(endpointConfig.QueueLimit.Value);
            }

            if (endpointConfig.QueueMaxLengthBytes.HasValue)
            {
                cfg.UseQueueMaxLengthBytes(endpointConfig.QueueMaxLengthBytes.Value);
            }

            if (endpointConfig.Dynamic != null)
            {
                if (endpointConfig.Dynamic.Outgoing.HasValue)
                {
                    if (endpointConfig.Dynamic.Outgoing.Value)
                    {
                        cfg.Route(MessageLabel.Any).ConfiguredWith(builder => new DynamicRouteResolver(builder));
                    }
                }
            }

            if (endpointConfig.Qos != null)
            {
                if (endpointConfig.Qos.PrefetchCount.HasValue)
                {
                    cfg.SetDefaultQoS(endpointConfig.Qos.PrefetchCount.Value);
                }
            }

            foreach (IValidator validator in endpointConfig.Validators)
            {
                if (validator.Group)
                {
                    MessageValidatorGroup v = this.ResolveValidatorGroup(validator.Name);
                    cfg.RegisterValidators(v);
                }
                else
                {
                    IMessageValidator v = this.ResolveValidator(validator.Name);
                    cfg.RegisterValidator(v);
                }
            }

            foreach (IOutgoing outgoingElement in endpointConfig.Outgoing)
            {
                var configurator = cfg.Route(outgoingElement.Label).WithAlias(outgoingElement.Key);

                if (outgoingElement.Confirm)
                {
                    configurator.WithConfirmation();

                    if (outgoingElement.ConfirmTimeout.HasValue)
                    {
                        configurator.WithConfirmationTimeout(outgoingElement.ConfirmTimeout.Value);
                    }
                }

                if (outgoingElement.Persist)
                {
                    configurator.Persistently();
                }

                if (outgoingElement.Ttl.HasValue)
                {
                    configurator.WithTtl(outgoingElement.Ttl.Value);
                }

                if (outgoingElement.CallbackEndpoint.Default)
                {
                    configurator.WithDefaultCallbackEndpoint();
                }

                if (outgoingElement.Timeout.HasValue)
                {
                    configurator.WithRequestTimeout(outgoingElement.Timeout);
                }

                if (outgoingElement.Delayed)
                {
                    configurator.Delayed();
                }

                if (outgoingElement.Direct)
                {
                    configurator.Direct();
                }

                // Connection string
                var connectionString = endpointConfig.ConnectionString;
                if (!string.IsNullOrEmpty(outgoingElement.ConnectionString))
                {
                    connectionString = outgoingElement.ConnectionString;
                }
                connectionString = connectionStringProvider?.GetConnectionString(outgoingElement.Label.ToMessageLabel()) ?? connectionString;


                configurator.WithConnectionString(connectionString);

                // Reuse connection
                if (outgoingElement.ReuseConnection.HasValue)
                {
                    configurator.ReuseConnection(outgoingElement.ReuseConnection.Value);
                }
            }

            foreach (IIncoming incomingElement in endpointConfig.Incoming)
            {
                var configurator = cfg.On(incomingElement.Label).WithAlias(incomingElement.Key);

                uint size = 0;
                ushort count = 50;

                // This should be the default values provided by RabbitMQ configurator (BusConsumerConfigurationEx);
                var qos = configurator.GetQoS();
                if (qos.HasValue)
                {
                    size = qos.Value.PrefetchSize;
                    count = qos.Value.PrefetchCount;
                }

                // Prefetch size
                if (endpointConfig.Qos.PrefetchSize.HasValue)
                {
                    size = endpointConfig.Qos.PrefetchSize.Value;
                }

                if (incomingElement.Qos.PrefetchSize.HasValue)
                {
                    size = incomingElement.Qos.PrefetchSize.Value;
                }

                // Prefetch count
                if (endpointConfig.Qos.PrefetchCount.HasValue)
                {
                    count = endpointConfig.Qos.PrefetchCount.Value;
                }

                if (incomingElement.Qos.PrefetchCount.HasValue)
                {
                    count = incomingElement.Qos.PrefetchCount.Value;
                }

                configurator.WithQoS(new QoSParams(count, size));

                // Parallelism level
                if (endpointConfig.ParallelismLevel.HasValue)
                {
                    configurator.WithParallelismLevel(endpointConfig.ParallelismLevel.Value);
                }

                if (incomingElement.ParallelismLevel.HasValue)
                {
                    configurator.WithParallelismLevel(incomingElement.ParallelismLevel.Value);
                }

                //Queue limit
                if (endpointConfig.QueueLimit.HasValue)
                {
                    configurator.WithQueueLimit(endpointConfig.QueueLimit.Value);
                }

                if (incomingElement.QueueLimit.HasValue)
                {
                    configurator.WithQueueLimit(incomingElement.QueueLimit.Value);
                }

                //Queue max length bytes
                if (endpointConfig.QueueMaxLengthBytes.HasValue)
                {
                    configurator.WithQueueMaxLengthBytes(endpointConfig.QueueMaxLengthBytes.Value);
                }

                if (incomingElement.QueueMaxLengthBytes.HasValue)
                {
                    configurator.WithQueueMaxLengthBytes(incomingElement.QueueMaxLengthBytes.Value);
                }

                // Accept
                if (incomingElement.RequiresAccept)
                {
                    configurator.RequiresAccept();
                }

                // Delayed
                if (incomingElement.Delayed)
                {
                    configurator.Delayed();
                }

                if (incomingElement.Direct)
                {
                    configurator.Direct(incomingElement.DirectId);
                }

                // Connection string
                var connectionString = endpointConfig.ConnectionString;
                if (!string.IsNullOrEmpty(incomingElement.ConnectionString))
                {
                    connectionString = incomingElement.ConnectionString;
                }
                connectionString = connectionStringProvider?.GetConnectionString(incomingElement.Label.ToMessageLabel()) ?? connectionString;


                configurator.WithConnectionString(connectionString);

                // Reuse connection
                if (incomingElement.ReuseConnection.HasValue)
                {
                    configurator.ReuseConnection(incomingElement.ReuseConnection.Value);
                }

                Type messageType = typeof(ExpandoObject);
                if (!string.IsNullOrWhiteSpace(incomingElement.Type))
                {
                    messageType = ResolveType(incomingElement.Type);
                }

                var consumerFactory = this.BuildConsumerFactory(incomingElement.React, messageType);

                object consumer = BuildConsumer(consumerFactory, messageType, incomingElement.Lifestyle);

                RegisterConsumer(configurator, messageType, consumer);

                if (!string.IsNullOrWhiteSpace(incomingElement.Validate))
                {
                    IMessageValidator validator = this.ResolveValidator(incomingElement.Validate, messageType);

                    configurator.WhenVerifiedBy(validator);
                }
            }

            return cfg;
        }

        /// <summary>
        /// The get event.
        /// </summary>
        /// <param name="endpointName">
        /// The endpoint name.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetEvent(string endpointName, string key)
        {
            Configuration.IEndpoint endpoint = this.GetEndPointByName(endpointName);
            IEnumerable<Configuration.IMessage> messages = endpoint.Outgoing
                .Cast<Configuration.IMessage>()
                .Concat(endpoint.Incoming);

            // NOTE: Если такого не будет, упадет соответствующий эксэпшн.
            return messages.First(x => x.Key == key).
                Label;
        }

        /// <summary>
        /// The get request config.
        /// </summary>
        /// <param name="endpointName">
        /// The endpoint name.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="IRequestConfiguration"/>.
        /// </returns>
        public IRequestConfiguration GetRequestConfig(string endpointName, string key)
        {
            Configuration.IEndpoint endpoint = this.GetEndPointByName(endpointName);

            IOutgoing reqDeclaration = endpoint
                .Outgoing
                .First(x => x.Key == key);

            return new RequestConfiguration(reqDeclaration.Timeout, reqDeclaration.Persist, reqDeclaration.Ttl);
        }

        /// <summary>
        /// The build consumer.
        /// </summary>
        /// <param name="consumerFactory">
        /// The consumer factory.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <param name="lifestyle">
        /// The lifestyle.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="ConfigurationErrorsException">
        /// </exception>
        private static object BuildConsumer(Func<object> consumerFactory, Type messageType, Lifestyle? lifestyle)
        {
            Type consumerType;
            switch (lifestyle)
            {
                case null:
                case Lifestyle.Normal:
                    return consumerFactory();
                case Lifestyle.Lazy:
                    consumerType = typeof(LazyConsumerOf<>).MakeGenericType(messageType);
                    return Activator.CreateInstance(consumerType, consumerFactory);
                case Lifestyle.Delegated:
                    consumerType = typeof(FactoryConsumerOf<>).MakeGenericType(messageType);
                    return Activator.CreateInstance(consumerType, consumerFactory);
                default:
                    throw new ConfigurationErrorsException("Unknown or unsupported consumer lifestyle : [{0}].".FormatEx(lifestyle));
            }
        }

        // TODO: make less fragile

        /// <summary>
        /// The register consumer.
        /// </summary>
        /// <param name="configurator">
        /// The configurator.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <param name="consumer">
        /// The consumer.
        /// </param>
        private static void RegisterConsumer(IReceiverConfigurator configurator, Type messageType, object consumer)
        {
            var configuratorType = configurator.GetType();

            MethodInfo method = configuratorType.GetMethods().
                Single(
                    mi => // mi.Name == "ReactWith" && //avoiding binding to method name
                    mi.IsGenericMethod && mi.ContainsGenericParameters && mi.GetParameters()
                    .Count() == 1 && mi.GetParameters()
                    .First()
                    .ParameterType.Name == typeof(IConsumer<>).Name);

            method.MakeGenericMethod(messageType)
                .Invoke(configurator, new[] { consumer });
        }

        /// <summary>
        /// The resolve type.
        /// </summary>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        /// <exception cref="ConfigurationErrorsException">
        /// </exception>
        private static Type ResolveType(string messageType)
        {
            Type type = Type.GetType(messageType);
            if (type != null)
            {
                return type;
            }

            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == messageType);

            if (type != null)
            {
                return type;
            }

            throw new ConfigurationErrorsException($"Unknown type [{messageType}]");
        }

        /// <summary>
        /// The build consumer factory.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <returns>
        /// The <see cref="Func{TResult}"/>.
        /// </returns>
        private Func<object> BuildConsumerFactory(string name, Type messageType)
        {
            return () => this.ResolveConsumer(name, messageType);
        }

        /// <summary>
        /// The get end point by name.
        /// </summary>
        /// <param name="endpointName">
        /// The endpoint name.
        /// </param>
        /// <returns>
        /// The <see cref="EndpointElement"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private Configuration.IEndpoint GetEndPointByName(string endpointName)
        {
            Configuration.IEndpoint endpoint = this.endpointsConfig[endpointName];

            if (endpoint == null)
            {
                throw new ArgumentException($"Попытка найти конфигурацию для endpoint {endpointName} закончилось провалом, пожалуйста укажите необходимую информацию в конфигурации {ServiceBusSectionName}");
            }

            return endpoint;
        }

        /// <summary>
        /// The resolve consumer.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private object ResolveConsumer(string name, Type messageType)
        {
            return this.dependencyResolver.Resolve(name, typeof(IConsumer<>).MakeGenericType(messageType));
        }

        private IConnectionStringProvider GetConnectionStringProvider(string name)
        {
            return (IConnectionStringProvider)this.dependencyResolver.Resolve(name, typeof(IConnectionStringProvider));
        }

        /// <summary>
        /// The resolve lifecycle handler.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="IBusLifecycleHandler"/>.
        /// </returns>
        private IBusLifecycleHandler ResolveLifecycleHandler(string name)
        {
            return (IBusLifecycleHandler)this.dependencyResolver.Resolve(name, typeof(IBusLifecycleHandler));
        }

        /// <summary>
        /// The resolve validator.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="IMessageValidator"/>.
        /// </returns>
        private IMessageValidator ResolveValidator(string name)
        {
            return (IMessageValidator)this.dependencyResolver.Resolve(name, typeof(IMessageValidator));
        }

        /// <summary>
        /// The resolve validator.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <returns>
        /// The <see cref="IMessageValidator"/>.
        /// </returns>
        private IMessageValidator ResolveValidator(string name, Type messageType)
        {
            Type validatorType = typeof(IMessageValidatorOf<>).MakeGenericType(messageType);
            return (IMessageValidator)this.dependencyResolver.Resolve(name, validatorType);
        }

        /// <summary>
        /// The resolve validator group.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="MessageValidatorGroup"/>.
        /// </returns>
        private MessageValidatorGroup ResolveValidatorGroup(string name)
        {
            return (MessageValidatorGroup)this.dependencyResolver.Resolve(name, typeof(MessageValidatorGroup));
        }

        /// <summary>
        /// Resolves a producer selector by the provided selector name
        /// </summary>
        /// <param name="name">The name identifying the producer selector</param>
        /// <returns><see cref="IProducerSelector"/></returns>
        private IProducerSelectorBuilder ResolveProducerSelectorBuilder(string name)
        {
            return (IProducerSelectorBuilder)this.dependencyResolver.Resolve(name, typeof(IProducerSelectorBuilder));
        }
    }
}
