using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Contour.Configuration;
using Contour.Receiving;
using Contour.Receiving.Consumers;
using Contour.Sending;
using Contour.Validation;

using Contour.Transport.RabbitMQ;
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
        private readonly EndpointsSection endpointsConfig;

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
        /// <param name="dependencyResolver">
        /// The dependency resolver.
        /// </param>
        internal AppConfigConfigurator(EndpointsSection endpointsConfig, IDependencyResolver dependencyResolver)
        {
            this.endpointsConfig = endpointsConfig;
            this.dependencyResolver = dependencyResolver;
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
        internal AppConfigConfigurator(EndpointsSection endpointsConfig, DependencyResolverFunc dependencyResolverFunc)
            : this(endpointsConfig, new LambdaDependencyResolver(dependencyResolverFunc))
        {
        }

        /// <summary>
        ///   Имена точек подключения к шине.
        /// </summary>
        public IEnumerable<string> Endpoints
        {
            get
            {
                return this.endpointsConfig.Endpoints.OfType<EndpointElement>()
                    .Select(e => e.Name);
            }
        }

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

            EndpointElement endpointConfig = this.GetEndPointByName(endpointName);

            cfg.SetEndpoint(endpointConfig.Name);

            cfg.SetConnectionString(endpointConfig.ConnectionString);

            if (!string.IsNullOrWhiteSpace(endpointConfig.LifecycleHandler))
            {
                cfg.HandleLifecycleWith(this.ResolveLifecycleHandler(endpointConfig.LifecycleHandler));
            }

            if (endpointConfig.Caching != null && endpointConfig.Caching.Enabled)
            {
                cfg.EnableCaching();
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

            if (endpointConfig.Dynamic != null)
            {
                if (endpointConfig.Dynamic.Outgoing.HasValue)
                {
                    if (endpointConfig.Dynamic.Outgoing.Value)
                    {
                        cfg.Route(MessageLabel.Any)
                            .ConfiguredWith(builder => new LambdaRouteResolver(
                                    (endpoint, label) =>
                                        {
                                            builder.Topology.Declare(
                                                Exchange.Named(label.Name)
                                                    .Durable.Fanout);
                                            return new RabbitRoute(label.Name);
                                        }));
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

            #region Validation

            foreach (ValidatorElement validator in endpointConfig.Validators)
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
            #endregion

            #region Outgoing

            foreach (OutgoingElement message in endpointConfig.Outgoing)
            {
                ISenderConfigurator senderCfg = cfg.Route(message.Label).
                    WithAlias(message.Key);

                if (message.Confirm)
                {
                    senderCfg.WithConfirmation();
                }

                if (message.Persist)
                {
                    senderCfg.Persistently();
                }

                if (message.Ttl.HasValue)
                {
                    senderCfg.WithTtl(message.Ttl.Value);
                }

                if (message.CallbackEndpoint.Default)
                {
                    senderCfg.WithDefaultCallbackEndpoint();
                }

                if (message.Timeout.HasValue)
                {
                    senderCfg.WithRequestTimeout(message.Timeout);
                }
            }
            #endregion

            #region Incoming

            foreach (IncomingElement incomingElement in endpointConfig.Incoming)
            {
                var configurator = cfg.On(incomingElement.Label).
                    WithAlias(incomingElement.Key);

                //this should be the default value
                var qos = configurator.GetQoS();

                if (qos.HasValue)
                {
                    var size = qos.Value.PrefetchSize;
                    var count = qos.Value.PrefetchCount;
                    
                    if (endpointConfig.Qos.PrefetchSize.HasValue)
                    {
                        size = endpointConfig.Qos.PrefetchSize.Value;

                        if (incomingElement.Qos.PrefetchSize.HasValue)
                        {
                            size = incomingElement.Qos.PrefetchSize.Value;
                        }
                    }

                    if (endpointConfig.Qos.PrefetchCount.HasValue)
                    {
                        count = endpointConfig.Qos.PrefetchCount.Value;

                        if (incomingElement.Qos.PrefetchCount.HasValue)
                        {
                            count = incomingElement.Qos.PrefetchCount.Value;
                        }
                    }

                    configurator.WithQoS(new QoSParams(count, size));
                }

                if (endpointConfig.ParallelismLevel.HasValue)
                {
                    var level = endpointConfig.ParallelismLevel.Value;

                    if (incomingElement.ParallelismLevel.HasValue)
                    {
                        level = incomingElement.ParallelismLevel.Value;
                    }

                    configurator.WithParallelismLevel(level);
                }
                
                if (incomingElement.RequiresAccept)
                {
                    configurator.RequiresAccept();
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
            #endregion

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
            EndpointElement endpoint = this.GetEndPointByName(endpointName);
            IEnumerable<MessageElement> messages = endpoint.Outgoing.Cast<MessageElement>().
                Concat(endpoint.Incoming.Cast<MessageElement>());

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
            EndpointElement endpoint = this.GetEndPointByName(endpointName);

            OutgoingElement reqDeclaration = endpoint.Outgoing.Cast<OutgoingElement>().
                First(x => x.Key == key);

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
            Type configuratorType = configurator.GetType();

            MethodInfo method = configuratorType.GetMethods().
                Single(
                    mi => // mi.Name == "ReactWith" && //avoiding binding to method name
                    mi.IsGenericMethod && mi.ContainsGenericParameters && mi.GetParameters()
                    .Count() == 1 && mi.GetParameters()
                    .First()
                    .ParameterType.Name == typeof(IConsumerOf<>).Name);

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

            throw new ConfigurationErrorsException(string.Format("Unknown type [{0}]", messageType));
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
        private EndpointElement GetEndPointByName(string endpointName)
        {
            EndpointElement endpoint = this.endpointsConfig.Endpoints[endpointName];

            if (endpoint == null)
            {
                throw new ArgumentException(string.Format("Попытка найти конфигурацию для endpoint {0} закончилось провалом, пожалуйста укажите необходимую информацию в конфигурации {1}", endpointName, ServiceBusSectionName));
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
            return this.dependencyResolver.Resolve(name, typeof(IConsumerOf<>).MakeGenericType(messageType));
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
    }
}
