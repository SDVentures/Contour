using Contour.Configuration.Configurator;

namespace Contour.Configurator.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using FluentAssertions;

    using NUnit.Framework;

    /// <summary>
    /// The configuration section reading specs.
    /// Please note that setting IsRequired=true on derived configuration elements does not take effect due to configuration system limitations. See https://social.msdn.microsoft.com/Forums/vstudio/en-US/710c69e7-0c70-4905-8a5d-448c1e12a2e5/loading-custom-configurationsection-ignores-isrequired-attribute-on-children-which-are-of?forum=netfxbcl for explanation.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."),
     SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class ConfigSectionReadingSpecs
    {
        /// <summary>
        /// The when_declaring_complex_configuration.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_complex_configuration
        {
            /// <summary>
            /// The should_read_everything.
            /// </summary>
            [Test(Description = "A shortcut cover-all test. No time. Decompose.")]
            public void should_read_everything()
            {
                const string config = @"<endpoints>
							<endpoint name=""tester"" connectionString=""amqp://localhost:666"">
								<incoming>
									<on key=""a"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" />
									<on key=""b"" label=""msg.b"" react=""TransformB"" />
								</incoming>
								<outgoing>
									<route key=""a"" label=""msg.out.a"" persist=""true"">
										<callbackEndpoint default=""true"" />
									</route>
								</outgoing>
							</endpoint>
						</endpoints>";

                var section = new XmlEndpointsSection(config);

                section.Endpoints.Should().
                    HaveCount(1);

                EndpointElement endpoint = section.Endpoints["tester"];
                endpoint.Incoming.Should().
                    HaveCount(2);
                endpoint.Outgoing.Should().
                    HaveCount(1);

                List<IncomingElement> incoming = endpoint.Incoming.OfType<IncomingElement>().
                    ToList();

                incoming[0].RequiresAccept.Should().
                    BeTrue();
                incoming[1].RequiresAccept.Should().
                    BeFalse();
            }
        }

        /// <summary>
        /// The when_declaring_endpoint_with_name.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_endpoint_with_name
        {
            /// <summary>
            /// The should_read_endpoint.
            /// </summary>
            [Test]
            public void should_read_endpoint()
            {
                const string config = @"<endpoints>
							<endpoint name=""tester"" connectionString=""amqp://localhost:666"">
							</endpoint>
						</endpoints>";

                var section = new XmlEndpointsSection(config);

                section.Endpoints.Should().
                    HaveCount(1);
                section.Endpoints["tester"].Should().
                    NotBeNull();
            }
        }

        /// <summary>
        /// The when_declaring_endpoint_without_name.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_endpoint_without_name
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                const string config = @"<endpoints>
							<endpoint connectionString=""amqp://localhost:666"">
							</endpoint>
						</endpoints>";

                Action readingConfig = () => new XmlEndpointsSection(config);

                readingConfig.ShouldThrow<ConfigurationErrorsException>();
            }
        }

        /// <summary>
        /// The when_declaring_multiple_endpoints_with_different_names.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_multiple_endpoints_with_different_names
        {
            /// <summary>
            /// The should_read_endpoints.
            /// </summary>
            [Test]
            public void should_read_endpoints()
            {
                const string config = @"<endpoints>
							<endpoint name=""a"" connectionString=""amqp://localhost:666"">
							</endpoint>
							<endpoint name=""b"" connectionString=""amqp://localhost:777"">
							</endpoint>
						</endpoints>";

                var section = new XmlEndpointsSection(config);

                section.Endpoints.Should().
                    HaveCount(2);
                section.Endpoints["a"].Should().
                    NotBeNull();
                section.Endpoints["b"].Should().
                    NotBeNull();
            }
        }

        /// <summary>
        /// The when_declaring_multiple_endpoints_with_same_name.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_multiple_endpoints_with_same_name
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                const string config = @"<endpoints>
							<endpoint name=""tester"" connectionString=""amqp://localhost:666"">
							</endpoint>
							<endpoint name=""tester"" connectionString=""amqp://localhost:777"">
							</endpoint>
						</endpoints>";

                Action readingConfig = () => new XmlEndpointsSection(config);

                readingConfig.ShouldThrow<ConfigurationErrorsException>();
            }
        }
        
        /// <summary>
        /// The when_endpoint_lifecycle_handler_is_provided.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_endpoint_lifecycle_handler_is_provided
        {
            /// <summary>
            /// The should_read_handler_name.
            /// </summary>
            [Test]
            public void should_read_handler_name()
            {
                const string config = @"<endpoints>
							<endpoint name=""Tester"" connectionString=""amqp://localhost:666"" lifecycleHandler=""handler"" />
						</endpoints>";

                var section = new XmlEndpointsSection(config);

                section.Endpoints["Tester"].LifecycleHandler.Should().
                    Be("handler");
            }
        }

        /// <summary>
        /// Тестовая конфигурация, которая проверяет корректность работы конфигурационных настроек QoS.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_qos_for_endpoint
        {
            /// <summary>
            /// Если задан QoS для конечной точки, тогда можно получить описание из конфигурации.
            /// </summary>
            [Test]
            public void should_read_configuration_property()
            {
                const string Config = 
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"">
                            <qos prefetchCount=""8"" />
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.AreEqual(8, section.Endpoints["a"].Qos.PrefetchCount, "Должно быть установлено значение QoS.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация, которая проверяет корректность работы конфигурационных настроек количества обработчиков сообщений.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_parallelismlevel_for_endpoint
        {
            /// <summary>
            /// Если задано количество обработчиков сообщений для каждой очереди конечной точки, тогда можно получить описание из конфигурации.
            /// </summary>
            [Test]
            public void should_read_configuration_property()
            {
                const string Config =
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"" parallelismLevel=""8"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.AreEqual(8, section.Endpoints["a"].ParallelismLevel, "Должно быть установлено количество обработчиков запроса.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация, которая проверяет корректность работы конфигурационных настроек динамической маршрутизации исходящих сообщений.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_dynamic_outgoing
        {
            /// <summary>
            /// Если задана динамическая маршрутизация исходящих сообщений для каждой конечной точки, тогда можно получить описание из конфигурации.
            /// </summary>
            [Test]
            public void should_read_configuration_property()
            {
                const string Config =
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"">
                            <dynamic outgoing=""true"" />
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.IsNotNull(section.Endpoints["a"].Dynamic.Outgoing, "Должны быть добавлены установки динамической маршрутизации.");
                Assert.IsTrue(section.Endpoints["a"].Dynamic.Outgoing.Value, "Должна быть включена динамическая маршрутизация исходящих сообщений.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация, которая проверяет корректность работы конфигурационных настроек времени хранения сообщений в Fault очереди.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_fault_queue_ttl
        {
            /// <summary>
            /// Если задано время хранения сообщений в Fault очереди для каждой очереди конечной точки, тогда можно получить описание из конфигурации.
            /// </summary>
            [Test]
            public void should_read_configuration_property()
            {
                const string Config =
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"" faultQueueTtl=""00:10:00"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.AreEqual(TimeSpan.Parse("00:10:00"), section.Endpoints["a"].FaultQueueTtl, "Должно быть установлено время хранения сообщений в Fault очереди.");
            }

            /// <summary>
            /// Если не задано время хранения сообщений в Fault очереди для каждой очереди конечной точки, тогда используется значение по умолчанию.
            /// </summary>
            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Config =
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.IsNull(section.Endpoints["a"].FaultQueueTtl, "Время хранения сообщений в Fault очереди не должно быть установлено.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация, которая проверяет корректность работы конфигурационных настроек максимального количества сообщений в Fault очереди.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_fault_queue_limit
        {
            /// <summary>
            /// Если задано максимальное количество сообщений в Fault очереди для каждой очереди конечной точки, тогда можно получить описание из конфигурации.
            /// </summary>
            [Test]
            public void should_read_configuration_property()
            {
                const int queueLimit = 100;
                string config = string.Format(
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"" faultQueueLimit=""{0}"">
                        </endpoint>
                    </endpoints>",
                    queueLimit);

                var section = new XmlEndpointsSection(config);
                Assert.AreEqual(queueLimit, section.Endpoints["a"].FaultQueueLimit, "Должно быть установлено время хранения сообщений в Fault очереди.");
            }

            /// <summary>
            /// Если не задано максимальное количество сообщений в Fault очереди для каждой очереди конечной точки, тогда используется значение по умолчанию.
            /// </summary>
            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Config =
                    @"<endpoints>
                        <endpoint name=""a"" connectionString=""amqp://localhost:666"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                Assert.IsNull(section.Endpoints["a"].FaultQueueLimit, "Время хранения сообщений в Fault очереди не должно быть установлено.");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_declaring_qos_for_incoming
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string endpointName = "ep";
                const ushort prefetchCount = 5;
                const uint prefetchSize = 6;
                const string onKeyName = "key";

                string Config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"">
                                        <qos prefetchCount=""{prefetchCount}"" prefetchSize=""{prefetchSize}"" />
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[endpointName];
                var elements = endpoint.Incoming.OfType<IncomingElement>();

                var on = elements.First(e => e.Key == onKeyName);

                on.Qos.PrefetchCount.Should().Be(prefetchCount, "Incoming QoS prefetch count should be set");
                on.Qos.PrefetchSize.Should().Be(prefetchSize, "Incoming QoS prefetch size should be set");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_declaring_parallelism_level_for_incoming
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string endpointName = "ep";
                const string onKeyName = "key";
                const uint parallelismLevel = 7;

                string Config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" parallelismLevel=""{parallelismLevel}"">
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[endpointName];
                var elements = endpoint.Incoming.OfType<IncomingElement>();

                var on = elements.First(e => e.Key == onKeyName);
                on.ParallelismLevel.Should().Be(parallelismLevel, "Incoming parallelism level should be set");
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string endpointName = "ep";
                const string onKeyName = "key";

                string Config =
                    $@"<endpoints>
                        <endpoint name=""{endpointName}"" connectionString=""amqp://localhost:666"">
                                <incoming>
                                    <on key=""{onKeyName}"" label=""msg.a"" react=""DynamicHandler"" requiresAccept=""true"" >
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[endpointName];
                var elements = endpoint.Incoming.OfType<IncomingElement>();

                var on = elements.First(e => e.Key == onKeyName);
                on.ParallelismLevel.Should().BeNull("Incoming parallelism level should not be set");
            }
        }
        
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_endpoint_connection_string
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string config = @"<endpoints>
							<endpoint name=""Tester"" connectionString=""amqp://localhost:666"" />
						</endpoints>";

                var section = new XmlEndpointsSection(config);

                section.Endpoints["Tester"].ConnectionString.Should().
                    Be("amqp://localhost:666");
            }

            [Test]
            public void should_throw_if_not_set()
            {
                const string config = @"<endpoints>
                                            <endpoint name=""Tester"" />
                                        </endpoints>";

                Action readingConfig = () => new XmlEndpointsSection(config);
                readingConfig.ShouldThrow<ConfigurationErrorsException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_declaring_endpoint_connection_reuse
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string Name = "ep";
                string config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString=""amqp://localhost:666"" reuseConnection=""true"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(config);
                var property = section.Endpoints[Name].ReuseConnection;
                property.Should().HaveValue();
                property.Value.Should().BeTrue();
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Name = "ep";
                string config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString=""amqp://localhost:666"">
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(config);
                var property = section.Endpoints[Name].ReuseConnection;

                property.Should().BeTrue();
            }
        }


        [TestFixture]
        public class when_declaring_connection_string_for_incoming
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <incoming>
                                    <on key=""key"" label=""label"" react=""reactor"" requiresAccept=""true"" connectionString=""amqp://localhost:777"">
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Incoming.OfType<IncomingElement>();
                var property = elements.First().ConnectionString;
                
                property.Should().NotBeNullOrEmpty();
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <incoming>
                                    <on key=""key"" label=""label"" react=""reactor"" requiresAccept=""true"" >
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Incoming.OfType<IncomingElement>();
                var property = elements.First().ConnectionString;

                property.Should().BeNullOrEmpty();
            }
        }

        [TestFixture]
        public class when_declaring_connection_reuse_for_incoming
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <incoming>
                                    <on key=""key"" label=""label"" react=""reactor"" requiresAccept=""true"" reuseConnection=""true"">
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Incoming.OfType<IncomingElement>();
                var property = elements.First().ReuseConnection;

                property.Should().HaveValue();
                property.Value.Should().BeTrue();
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <incoming>
                                    <on key=""key"" label=""label"" react=""reactor"" requiresAccept=""true"" >
                                    </on>
                                </incoming>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Incoming.OfType<IncomingElement>();
                var property = elements.First().ReuseConnection;

                property.Should().NotHaveValue();
            }
        }

        [TestFixture]
        public class when_declaring_connection_string_for_outgoing
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <outgoing>
                                    <route key=""key"" label=""label"" connectionString=""amqp://localhost:999"">
									</route>
                                </outgoing>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Outgoing.OfType<OutgoingElement>();
                var property = elements.First().ConnectionString;

                property.Should().NotBeNullOrEmpty();
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <outgoing>
                                    <route key=""key"" label=""label"">
									</route>
                                </outgoing>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Outgoing.OfType<OutgoingElement>();
                var property = elements.First().ConnectionString;

                property.Should().BeNullOrEmpty();
            }
        }

        [TestFixture]
        public class when_declaring_connection_reuse_for_outgoing
        {
            [Test]
            public void should_read_configuration_property()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <outgoing>
                                    <route key=""key"" label=""label"" reuseConnection=""true"">
									</route>
                                </outgoing>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Outgoing.OfType<OutgoingElement>();
                var property = elements.First().ReuseConnection;

                property.Should().HaveValue();
                property.Should().BeTrue();
            }

            [Test]
            public void should_use_default_value_if_not_set()
            {
                const string Name = "ep";
                string Config =
                    $@"<endpoints>
                        <endpoint name=""{Name}"" connectionString="""">
                                <outgoing>
                                    <route key=""key"" label=""label"">
									</route>
                                </outgoing>
                        </endpoint>
                    </endpoints>";

                var section = new XmlEndpointsSection(Config);
                var endpoint = section.Endpoints[Name];
                var elements = endpoint.Outgoing.OfType<OutgoingElement>();
                var property = elements.First().ReuseConnection;

                property.Should().NotHaveValue();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
