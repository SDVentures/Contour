using System.Linq;
using Contour.Configuration;
using Contour.Helpers;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;

namespace Contour.Configurator.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Threading;

    using FluentAssertions;

    using FluentValidation;

    using Contour.Operators;
    using Contour.Receiving;
    using Contour.Receiving.Consumers;
    using Contour.Validation;
    using Contour.Validation.Fluent;

    using Moq;

    using Ninject;

    using NUnit.Framework;

    /// <summary>
    /// The bus configuration specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class BusConfigurationSpecs
    {
        /// <summary>
        /// The boo payload validator.
        /// </summary>
        public class BooPayloadValidator : FluentPayloadValidatorOf<BooMessage>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BooPayloadValidator"/>.
            /// </summary>
            public BooPayloadValidator()
            {
                this.RuleFor(x => x.Num).GreaterThan(100);
            }
        }

        /// <summary>
        /// The bus dependent handler.
        /// </summary>
        public class BusDependentHandler : IConsumerOf<BooMessage>
        {
            /// <summary>
            /// The build count.
            /// </summary>
            public static int BuildCount;

            /// <summary>
            /// The wait event.
            /// </summary>
            public static CountdownEvent WaitEvent;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BusDependentHandler"/>.
            /// </summary>
            /// <param name="bus">
            /// The bus.
            /// </param>
            public BusDependentHandler(IBus bus)
            {
                this.Bus = bus;

                BuildCount++;
            }

            /// <summary>
            /// Gets the bus.
            /// </summary>
            public IBus Bus { get; private set; }

            /// <summary>
            /// The reset.
            /// </summary>
            public static void Reset()
            {
                WaitEvent = new CountdownEvent(3);
                BuildCount = 0;
            }

            /// <summary>
            /// The handle.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public void Handle(IConsumingContext<BooMessage> context)
            {
                WaitEvent.Signal();
            }
        }

        /// <summary>
        /// The bus dependent transformer.
        /// </summary>
        public class BusDependentTransformer : IMessageOperator
        {
            /// <summary>
            /// The build count.
            /// </summary>
            public static int BuildCount;

            /// <summary>
            /// The wait event.
            /// </summary>
            public static CountdownEvent WaitEvent;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BusDependentTransformer"/>.
            /// </summary>
            /// <param name="bus">
            /// The bus.
            /// </param>
            public BusDependentTransformer(IBus bus)
            {
                this.Bus = bus;

                BuildCount++;
            }

            /// <summary>
            /// Gets the bus.
            /// </summary>
            public IBus Bus { get; private set; }

            /// <summary>
            /// The reset.
            /// </summary>
            public static void Reset()
            {
                WaitEvent = new CountdownEvent(3);
                BuildCount = 0;
            }

            public IEnumerable<IMessage> Apply(IMessage message)
            {
                WaitEvent.Signal();
                yield break;
            }
        }

        /// <summary>
        /// The concrete handler of.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        public class ConcreteHandlerOf<T> : IConsumerOf<T>
            where T : class
        {
            /// <summary>
            /// The _received.
            /// </summary>
            private readonly ManualResetEvent _received = new ManualResetEvent(false);

            /// <summary>
            /// Gets the received.
            /// </summary>
            public WaitHandle Received
            {
                get
                {
                    return this._received;
                }
            }

            /// <summary>
            /// The handle.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public void Handle(IConsumingContext<T> context)
            {
                this._received.Set();
            }
        }

        /// <summary>
        /// The concrete transformer of.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        public class ConcreteTransformerOf<T> : IMessageOperator
            where T : class
        {
            /// <summary>
            /// The _received.
            /// </summary>
            private readonly ManualResetEvent received = new ManualResetEvent(false);

            /// <summary>
            /// Gets the received.
            /// </summary>
            public WaitHandle Received
            {
                get
                {
                    return this.received;
                }
            }

            public IEnumerable<IMessage> Apply(IMessage message)
            {
                this.received.Set();
                yield break;
            }
        }

        /// <summary>
        /// The foo payload validator.
        /// </summary>
        public class FooPayloadValidator : FluentPayloadValidatorOf<FooMessage>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FooPayloadValidator"/>.
            /// </summary>
            public FooPayloadValidator()
            {
                this.RuleFor(x => x.Num).
                    LessThan(100);
            }
        }

        /// <summary>
        /// The when_configuring_endpoint_with_lifecycle_handler.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_configuring_endpoint_with_lifecycle_handler : RabbitMqFixture
        {
            /// <summary>
            /// The should_handle_state_changes.
            /// </summary>
            [Test]
            public void should_handle_state_changes()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                        <endpoint name=""producer"" connectionString=""{0}{1}"" lifecycleHandler=""ProducerHandler"">
                            <outgoing>
                                <route key=""a"" label=""msg.a"" />
                            </outgoing>
                        </endpoint>
                    </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                var handler = new Mock<IBusLifecycleHandler>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IBusLifecycleHandler>().
                    ToConstant(handler.Object).
                    Named("ProducerHandler");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                handler.Verify(h => h.OnStarting(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Once);
                handler.Verify(h => h.OnStarted(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Once);
                handler.Verify(h => h.OnStopping(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Never);
                handler.Verify(h => h.OnStopped(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Never);

                producer.Stop();

                handler.Verify(h => h.OnStopping(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Once);
                handler.Verify(h => h.OnStopped(It.IsAny<IBus>(), It.IsAny<EventArgs>()), Times.Once);
            }
        }

        /// <summary>
        /// The when_declaring_delegated_consumer.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_declaring_delegated_consumer : RabbitMqFixture
        {
            /// <summary>
            /// The should_build_consumer_each_time.
            /// </summary>
            [Test]
            public void should_build_consumer_each_time()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                        <endpoint name=""producer"" connectionString=""{0}{1}"">
                            <outgoing>
                                <route key=""a"" label=""msg.a"" />
                                <route key=""b"" label=""msg.b"" />
                            </outgoing>
                        </endpoint>
                    </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                string consumerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooHandler"" type=""BooMessage"" lifestyle=""Delegated"" />
                                    <on key=""b"" label=""msg.b"" react=""BooTransformer"" type=""BooMessage"" lifestyle=""Delegated"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                BusDependentHandler.Reset();
                BusDependentTransformer.Reset();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<BooMessage>>().
                    To<BusDependentHandler>().
                    InTransientScope().
                    Named("BooHandler");

                kernel.Bind<IMessageOperator>().To<BusDependentTransformer>();

                kernel.Bind<IConsumerOf<BooMessage>>().To<OperatorConsumerOf<BooMessage>>()
                    .InTransientScope()
                    .Named("BooTransformer");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                IBus consumer = this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                kernel.Bind<IBus>().ToConstant(consumer);

                producer.Emit("msg.a", new { Num = 13 });
                producer.Emit("msg.a", new { Num = 13 });
                producer.Emit("msg.a", new { Num = 13 });

                BusDependentHandler.WaitEvent.Wait(5.Seconds()).Should().BeTrue();
                BusDependentHandler.BuildCount.Should().Be(3);

                producer.Emit("msg.b", new { Num = 13 });
                producer.Emit("msg.b", new { Num = 13 });
                producer.Emit("msg.b", new { Num = 13 });

                BusDependentTransformer.WaitEvent.Wait(5.Seconds()).
                    Should().
                    BeTrue();
                BusDependentTransformer.BuildCount.Should().
                    Be(3);
            }
        }

        /// <summary>
        /// The when_declaring_lazy_consumer.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_declaring_lazy_consumer : RabbitMqFixture
        {
            /// <summary>
            /// The should_build_consumer_once.
            /// </summary>
            [Test]
            public void should_build_consumer_once()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                    <route key=""b"" label=""msg.b"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                string consumerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooHandler"" type=""BooMessage"" lifestyle=""Lazy"" />
                                    <on key=""b"" label=""msg.b"" react=""BooTransformer"" type=""BooMessage"" lifestyle=""Lazy"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                BusDependentHandler.Reset();
                BusDependentTransformer.Reset();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<BooMessage>>().
                    To<BusDependentHandler>().
                    InTransientScope().
                    Named("BooHandler");
                kernel.Bind<IMessageOperator>().To<BusDependentTransformer>();

                kernel.Bind<IConsumerOf<BooMessage>>().To<OperatorConsumerOf<BooMessage>>()
                    .InTransientScope()
                    .Named("BooTransformer");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                IBus consumer = this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                kernel.Bind<IBus>().
                    ToConstant(consumer);

                producer.Emit("msg.a", new { Num = 13 });
                producer.Emit("msg.a", new { Num = 13 });
                producer.Emit("msg.a", new { Num = 13 });

                BusDependentHandler.WaitEvent.Wait(5.Seconds()).
                    Should().
                    BeTrue();
                BusDependentHandler.BuildCount.Should().
                    Be(1);

                producer.Emit("msg.b", new { Num = 13 });
                producer.Emit("msg.b", new { Num = 13 });
                producer.Emit("msg.b", new { Num = 13 });

                BusDependentTransformer.WaitEvent.Wait(5.Seconds()).
                    Should().
                    BeTrue();
                BusDependentTransformer.BuildCount.Should().
                    Be(1);
            }
        }

        /// <summary>
        /// The when_receiving_with_configured_concrete_consumer.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_with_configured_concrete_consumer : RabbitMqFixture
        {
            /// <summary>
            /// The should_receive.
            /// </summary>
            [Test]
            public void should_receive()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                string consumerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooHandler"" type=""BooMessage"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                    this.AmqpConnection,
                    this.VhostName);

                var handler = new ConcreteHandlerOf<BooMessage>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<BooMessage>>().
                    ToConstant(handler).
                    Named("BooHandler");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                producer.Emit("msg.a", new { Num = 13 });

                handler.Received.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();
            }
        }

        /// <summary>
        /// The when_receiving_with_configured_dynamic_consumer.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_with_configured_dynamic_consumer : RabbitMqFixture
        {
            /// <summary>
            /// The should_receive.
            /// </summary>
            [Test]
            public void should_receive()
            {
                string producerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                string consumerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""DynamicHandler"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                var handler = new ConcreteHandlerOf<ExpandoObject>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<ExpandoObject>>().
                    ToConstant(handler).
                    Named("DynamicHandler");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                producer.Emit("msg.a", new { This = "That" });

                handler.Received.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();
            }
        }

        /// <summary>
        /// The when_receiving_with_configured_validator_on_global_declaration.
        /// </summary>
        [TestFixture]
        [Category("Integration")]

        // [Ignore("WIP")]
        public class when_receiving_with_configured_validator_on_global_declaration : RabbitMqFixture
        {
            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                string producerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                string consumerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <validators>
                                    <add name=""ValidatorGroup"" group=""true"" />
                                </validators>
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooHandler"" type=""BooMessage"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                var handler = new ConcreteHandlerOf<BooMessage>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<BooMessage>>()
                    .ToConstant(handler)
                    .InSingletonScope()
                    .Named("BooHandler");
                kernel.Bind<IMessageValidator>()
                    .To<BooPayloadValidator>()
                    .InSingletonScope();
                kernel.Bind<IMessageValidator>()
                    .To<FooPayloadValidator>()
                    .InSingletonScope();

                kernel.Bind<MessageValidatorGroup>()
                    .ToSelf()
                    .InSingletonScope()
                    .Named("ValidatorGroup")
                    .WithConstructorArgument("validators", ctx => ctx.Kernel.GetAll<IMessageValidator>());

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                producer.Emit("msg.a", new { Num = 13 });

                handler.Received.WaitOne(3.Seconds()).
                    Should().
                    BeFalse();
            }
        }

        /// <summary>
        /// The when_receiving_with_configured_validator_on_incoming_declaration.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_with_configured_validator_on_incoming_declaration : RabbitMqFixture
        {
            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                string producerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                string consumerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooHandler"" type=""BooMessage"" validate=""BooValidator"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                var handler = new ConcreteHandlerOf<BooMessage>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IConsumerOf<BooMessage>>()
                    .ToConstant(handler)
                    .InSingletonScope()
                    .Named("BooHandler");
                kernel.Bind<IMessageValidatorOf<BooMessage>>()
                    .To<BooPayloadValidator>()
                    .InSingletonScope()
                    .Named("BooValidator");

                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                producer.Emit("msg.a", new { Num = 13 });

                handler.Received.WaitOne(3.Seconds())
                    .Should()
                    .BeFalse();
            }
        }

        /// <summary>
        /// The when_transforming_with_configured_concrete_transformer.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_transforming_with_configured_concrete_transformer : RabbitMqFixture
        {
            /// <summary>
            /// The should_receive.
            /// </summary>
            [Test]
            public void should_receive()
            {
                string producerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}{1}"">
                                <outgoing>
                                    <route key=""a"" label=""msg.a"" />
                                </outgoing>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                string consumerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""consumer"" connectionString=""{0}{1}"">
                                <incoming>
                                    <on key=""a"" label=""msg.a"" react=""BooTransformer"" type=""BooMessage"" />
                                </incoming>
                            </endpoint>
                        </endpoints>",
                        this.AmqpConnection,
                        this.VhostName);

                var handler = new ConcreteTransformerOf<BooMessage>();

                IKernel kernel = new StandardKernel();
                kernel.Bind<IMessageOperator>().ToConstant(handler);

                kernel.Bind<IConsumerOf<BooMessage>>().To<OperatorConsumerOf<BooMessage>>()
                    .InTransientScope()
                    .Named("BooTransformer");


                DependencyResolverFunc dependencyResolver = (name, type) => kernel.Get(type, name);

                this.StartBus(
                    "consumer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(consumerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("consumer", cfg);
                        });

                IBus producer = this.StartBus(
                    "producer", 
                    cfg =>
                        {
                            var section = new XmlEndpointsSection(producerConfig);
                            new AppConfigConfigurator(section, dependencyResolver).Configure("producer", cfg);
                        });

                producer.Emit("msg.a", new { Num = 13 });

                handler.Received.WaitOne(5.Seconds())
                    .Should()
                    .BeTrue();
            }
        }

        /// <summary>
        /// При указании конечной точки.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint
        {
            /// <summary>
            /// Можно установить значение QoS.
            /// </summary>
            [Test]
            public void should_set_qos()
            {
                const string ProducerConfig = 
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" lifecycleHandler=""ProducerHandler"">
                                <qos prefetchCount=""8"" />
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();
                busConfigurator.UseRabbitMq();

                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                RabbitReceiverOptions rabbitReceiverOptions = ((BusConfiguration)result).ReceiverDefaults as RabbitReceiverOptions;
                Assert.IsNotNull(rabbitReceiverOptions, "Долны быть установлены настройки получателя.");
                Maybe<QoSParams> qosMaybe = rabbitReceiverOptions.GetQoS();
                Assert.IsTrue(qosMaybe.HasValue, "QoS должен быть установлен.");
                Assert.AreEqual(8, qosMaybe.Value.PrefetchCount, "Должно быть установлено количество потоков.");
            }

            /// <summary>
            /// Можно использовать значение по умолчанию.
            /// </summary>
            [Test]
            public void should_be_default()
            {
                const string ProducerConfig =
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" lifecycleHandler=""ProducerHandler"">
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();

                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);

                using (var bus = new BusFactory().Create(cfg => sut.Configure("producer", cfg), false))
                {
                    RabbitReceiverOptions rabbitReceiverOptions = ((BusConfiguration)bus.Configuration).ReceiverDefaults as RabbitReceiverOptions;
                    Assert.IsNotNull(rabbitReceiverOptions, "Долны быть установлены настройки получателя.");
                    Maybe<QoSParams> qosMaybe = rabbitReceiverOptions.GetQoS();
                    Assert.IsTrue(qosMaybe.HasValue, "QoS должен быть установлен.");
                    Assert.AreEqual(50, qosMaybe.Value.PrefetchCount, "Должно быть установлено количество потоков.");
                }

            }
        }

        /// <summary>
        /// Если в конфигурации установлено количество обработчиков сообщений из очередей конечной точки.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint_with_parallelism_level
        {
            /// <summary>
            /// Тогда это значение должно быть использовано при конфигурации.
            /// </summary>
            [Test]
            public void should_set_parallelismLevel()
            {
                const string ProducerConfig =
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" parallelismLevel=""8"">
                                <qos prefetchCount=""8"" />
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();
                busConfigurator.UseRabbitMq();

                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                ReceiverOptions receiverOptions = ((BusConfiguration)result).ReceiverDefaults;
                Assert.IsTrue(receiverOptions.GetParallelismLevel().HasValue, "Должно быть установлено количество обработчиков.");
                Assert.AreEqual(8, receiverOptions.GetParallelismLevel().Value, "Должно быть установлено количество обработчиков.");
            }
        }

        /// <summary>
        /// Если в конфигурации время хранения сообщений в Fault очереди.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint_with_fault_queue_ttl
        {
            /// <summary>
            /// Тогда это значение должно быть использовано при конфигурации.
            /// </summary>
            [Test]
            public void should_set_queue_ttl()
            {
                const string ProducerConfig =
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" faultQueueTtl=""10:10:00"">
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();

                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                ReceiverOptions receiverOptions = ((BusConfiguration)result).ReceiverDefaults;
                Assert.IsTrue(receiverOptions.GetFaultQueueTtl().HasValue, "Должно быть установлено время хранения сообщений.");
                Assert.AreEqual(TimeSpan.Parse("10:10:00"), receiverOptions.GetFaultQueueTtl().Value, "Должно быть устрановлено корректное время хранения.");
            }

            /// <summary>
            /// Тогда это значение не должно быть установлено по умолчанию.
            /// </summary>
            [Test]
            public void should_not_be_set_by_default()
            {
                const string ProducerConfig =
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"">
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();

                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                ReceiverOptions receiverOptions = ((BusConfiguration)result).ReceiverDefaults;
                Assert.IsFalse(receiverOptions.GetFaultQueueTtl().HasValue, "Не должно быть установлено время хранения сообщений.");
            }
        }

        /// <summary>
        /// Если в конфигурации ограничено количество сообщений в Fault очереди.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint_with_fault_queue_limit
        {
            /// <summary>
            /// Тогда это значение должно быть использовано при конфигурации.
            /// </summary>
            [Test]
            public void should_set_queue_limit()
            {
                const int queueLimit = 100;
                string producerConfig = string.Format(
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" faultQueueLimit=""{0}"">
                            </endpoint>
                        </endpoints>", 
                        queueLimit);

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                ReceiverOptions receiverOptions = ((BusConfiguration)result).ReceiverDefaults;
                Assert.IsTrue(receiverOptions.GetFaultQueueLimit().HasValue, "Должно быть установлено максимальное количество сообщений.");
                Assert.AreEqual(queueLimit, receiverOptions.GetFaultQueueLimit().Value, "Должно быть устрановлено корректное максимальное количество сообщений.");
            }

            /// <summary>
            /// Тогда это значение не должно быть установлено по умолчанию.
            /// </summary>
            [Test]
            public void should_not_be_set_by_default()
            {
                string producerConfig = 
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"">
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var busConfigurator = new BusConfiguration();

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);
                var result = sut.Configure("producer", busConfigurator);

                ReceiverOptions receiverOptions = ((BusConfiguration)result).ReceiverDefaults;
                Assert.IsFalse(receiverOptions.GetFaultQueueLimit().HasValue, "Не должно быть установлено максимальное количество сообщений.");

            }
        }

        /// <summary>
        /// Если в конфигурации динамическая исходящая маршрутизация.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint_with_dynamic_outgoing_routing
        {
            /// <summary>
            /// Тогда это значение должно быть использовано при конфигурации.
            /// </summary>
            [Test]
            public void should_route_any()
            {
                const string ProducerConfig =
                        @"<endpoints>
                            <endpoint name=""producer"" connectionString=""amqp://localhost/integration"" >
                                <dynamic outgoing=""true"" />
                            </endpoint>
                        </endpoints>";

                Mock<IDependencyResolver> dependencyResoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(ProducerConfig);
                var sut = new AppConfigConfigurator(section, dependencyResoverMock.Object);

                using (var bus = new BusFactory().Create(cfg => sut.Configure("producer", cfg), false))
                {
                    Assert.IsTrue(bus.CanRoute(MessageLabel.Any), "Должна быть включена динамическая маршрутизация.");
                }
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_configuring_endpoint_incoming
        {
            [Test]
            public void should_set_qos_prefetch_count_if_present()
            {
                const string endpointName = "ep";
                const int prefetchCount = 5;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"">
                                        <qos prefetchCount=""{prefetchCount}"" />
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);
                
                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration) result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();
                var receiverOptions = (RabbitReceiverOptions) receiverConfiguration.Options;
                
                var qos = receiverOptions.GetQoS();
                var value = qos.Value;

                value.PrefetchCount.Should().Be(prefetchCount, "Incoming QoS prefetch count should be set");
            }

            [Test]
            public void should_set_qos_prefetch_size_if_present()
            {
                const string endpointName = "ep";
                const int prefetchSize = 6;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"">
                                        <qos prefetchCount="""" prefetchSize=""{prefetchSize}"" />
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();
                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;

                var qos = receiverOptions.GetQoS();
                var value = qos.Value;

                value.PrefetchSize.Should().Be(prefetchSize, "Incoming QoS prefetch size should be set");
            }

            [Test]
            public void should_use_endpoint_qos_prefetch_count_if_not_present()
            {
                const string endpointName = "ep";
                const int prefetchCount = 5;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                            <qos prefetchCount=""{prefetchCount}"" />
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;

                var qos = receiverOptions.GetQoS();
                var value = qos.Value;

                value.PrefetchCount.Should().Be(prefetchCount, "Endpoint QoS prefetch count should be used");
            }

            [Test]
            public void should_use_endpoint_qos_prefetch_size_if_not_present()
            {
                const string endpointName = "ep";
                const int prefetchSize = 6;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                            <qos prefetchCount="""" prefetchSize=""{prefetchSize}"" />
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;

                var qos = receiverOptions.GetQoS();
                var value = qos.Value;

                value.PrefetchSize.Should().Be(prefetchSize, "Endpoint QoS prefetch size should be used");
            }

            [Test]
            public void should_use_qos_default_prefetch_count_if_no_incoming_and_endpoint_settings_are_present()
            {
                const string endpointName = "ep";
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                            <incoming>
                                <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                            </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;
                var qos = receiverOptions.GetQoS().Value;

                var receiverDefaults = (RabbitReceiverOptions)busConfiguration.ReceiverDefaults;
                var defaultQos = receiverDefaults.GetQoS().Value;
                
                qos.PrefetchCount.Should().Be(defaultQos.PrefetchCount, "Default QoS prefetch count should be used");
            }

            [Test]
            public void should_use_qos_default_prefetch_size_if_no_incoming_and_endpoint_settings_are_present()
            {
                const string endpointName = "ep";
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                            <incoming>
                                <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                            </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;
                var qos = receiverOptions.GetQoS().Value;

                var receiverDefaults = (RabbitReceiverOptions)busConfiguration.ReceiverDefaults;
                var defaultQos = receiverDefaults.GetQoS().Value;

                qos.PrefetchSize.Should().Be(defaultQos.PrefetchSize, "Default QoS prefetch size should be used");
            }

            [Test]
            public void should_set_parallelism_level_if_present()
            {
                const string endpointName = "ep";
                const uint parallelismLevel = 99;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"" parallelismLevel=""123"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" parallelismLevel=""{parallelismLevel}""/>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;
                receiverOptions.GetParallelismLevel().Value.Should().Be(parallelismLevel, "Incoming parallelism level should be set");
            }

            [Test]
            public void should_use_endpoint_parallelism_level_if_not_present()
            {
                const string endpointName = "ep";
                const uint parallelismLevel = 99;
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"" parallelismLevel=""{parallelismLevel}"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;
                receiverOptions.GetParallelismLevel().Value.Should().Be(parallelismLevel, "Endpoint parallelism level should be used");
            }

            [Test]
            public void should_use_parallelism_level_defaults_if_no_incoming_and_endpoint_settings_are_present()
            {
                const string endpointName = "ep";
                const string onKeyName = "key";

                string config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"" >
                            <incoming>
                                <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
                            </incoming>
                        </endpoint>
                    </endpoints>";

                var resoverMock = new Mock<IDependencyResolver>();
                var section = new XmlEndpointsSection(config);
                var configurator = new AppConfigConfigurator(section, resoverMock.Object);

                var busConfiguration = new BusConfiguration();
                busConfiguration.UseRabbitMq(); //Basic receiver configurator and receiver options are actually unaware of any QoS settings; so these tests are not really Contour specific

                var result = configurator.Configure(endpointName, busConfiguration);
                var busConfigurationResult = (BusConfiguration)result;
                var receiverConfiguration = busConfigurationResult.ReceiverConfigurations.First();

                var receiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;
                var receiverDefaults = busConfiguration.ReceiverDefaults;

                receiverOptions.GetParallelismLevel().Value.Should().Be(receiverDefaults.GetParallelismLevel().Value, "Default parallelism level should be used");
            }
        }
    }

    public class BooMessage
    {
        public int Num { get; set; }
    }

    public class FooMessage
    {
        public int Num { get; set; }
    }

    // ReSharper restore InconsistentNaming
}
