using System;
using System.IO;
using System.Linq;
using System.Xml;

using Contour.Configurator;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// Specification to test fault strategies.
    /// </summary>
    class FaultStrategySpecs
    {
        /// <summary>
        /// When declaring endpoints
        /// </summary>
        [TestFixture]
        public class WhenDeclareEndpoint : RabbitMqFixture
        {
            /// <summary>
            /// Should be used fault queue ttl if it was specified.
            /// </summary>
            [Test]
            public void ShoudUseFaultQueueTtlIfSpecified()
            {
                const int ttlInDays = 3;
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}"" faultQueueTtl=""{1}"">
                            </endpoint>
                        </endpoints>", this.AmqpConnection + this.VhostName, TimeSpan.FromDays(ttlInDays));

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, (name, type) => null);

                IBus bus = this.CreateBus(() => new BusFactory().Create(cfg => sut.Configure("producer", cfg), true));
                bus.WhenReady.WaitOne();


                Testing.Plumbing.Queue queue = this.Broker.GetQueues(this.VhostName).First(q => q.Name == "producer.Fault");
                CollectionAssert.IsNotEmpty(queue.Arguments, "У очереди producer.Fault должны быть выставлены свойства.");
                Assert.IsTrue(queue.Arguments.ContainsKey("x-message-ttl"), "У очереди producer.Fault должно быть установлено время жизни.");
                Assert.AreEqual((ttlInDays * 24 * 60 * 60 * 1000).ToString(), queue.Arguments["x-message-ttl"], "Должно быть установлено время жизни из конфигурационного файла.");
            }

            /// <summary>
            /// Should be used fault queue ttl if it was specified.
            /// </summary>
            [Test]
            public void ShoudUseFaultQueueTtlDefaultIfNotSpecified()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}"">
                            </endpoint>
                        </endpoints>", this.AmqpConnection + this.VhostName);

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, (name, type) => null);

                IBus bus = this.CreateBus(() => new BusFactory().Create(cfg => sut.Configure("producer", cfg), true));
                bus.WhenReady.WaitOne();


                Testing.Plumbing.Queue queue = this.Broker.GetQueues(this.VhostName).First(q => q.Name == "producer.Fault");
                CollectionAssert.IsNotEmpty(queue.Arguments, "У очереди producer.Fault должны быть выставлены свойства.");
                Assert.IsTrue(queue.Arguments.ContainsKey("x-message-ttl"), "У очереди producer.Fault должно быть установлено время жизни.");
                Assert.AreEqual((21 * 24 * 60 * 60 * 1000).ToString(), queue.Arguments["x-message-ttl"], "Должно быть установлено время жизни по умолчанию.");
            }

            /// <summary>
            /// Should be used fault queue limit if it was specified.
            /// </summary>
            [Test]
            public void ShoudUseFaultQueueLimitIfSpecified()
            {
                const int limit = 3;
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}"" faultQueueLimit=""{1}"">
                            </endpoint>
                        </endpoints>", this.AmqpConnection + this.VhostName, limit);

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, (name, type) => null);

                IBus bus = this.CreateBus(() => new BusFactory().Create(cfg => sut.Configure("producer", cfg), true));
                bus.WhenReady.WaitOne();


                Testing.Plumbing.Queue queue = this.Broker.GetQueues(this.VhostName).First(q => q.Name == "producer.Fault");
                CollectionAssert.IsNotEmpty(queue.Arguments, "У очереди producer.Fault должны быть выставлены свойства.");
                Assert.IsTrue(queue.Arguments.ContainsKey("x-max-length"), "У очереди producer.Fault должно быть установлено ограничение на количество сообщений в очереди.");
                Assert.AreEqual((limit).ToString(), queue.Arguments["x-max-length"], "Должно быть установлено  ограничение на количество сообщений в очереди из конфигурационного файла.");
            }

            /// <summary>
            /// Should be used fault queue limit if it was specified.
            /// </summary>
            [Test]
            public void ShoudUseFaultQueueLimitDefaultIfNotSpecified()
            {
                string producerConfig = string.Format(
                    @"<endpoints>
                            <endpoint name=""producer"" connectionString=""{0}"">
                            </endpoint>
                        </endpoints>", this.AmqpConnection + this.VhostName);

                var section = new XmlEndpointsSection(producerConfig);
                var sut = new AppConfigConfigurator(section, (name, type) => null);

                IBus bus = this.CreateBus(() => new BusFactory().Create(cfg => sut.Configure("producer", cfg), true));
                bus.WhenReady.WaitOne();


                Testing.Plumbing.Queue queue = this.Broker.GetQueues(this.VhostName).First(q => q.Name == "producer.Fault");
                CollectionAssert.IsNotEmpty(queue.Arguments, "У очереди producer.Fault должны быть выставлены свойства.");
                Assert.IsFalse(queue.Arguments.ContainsKey("x-max-length"), "У очереди producer.Fault не должно быть установлено ограничение на количество сообщений в очереди.");
            }
        }

        internal class XmlEndpointsSection : EndpointsSection
        {
            #region Constructors and Destructors

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="XmlEndpointsSection"/>.
            /// </summary>
            /// <param name="configXml">
            /// The config xml.
            /// </param>
            public XmlEndpointsSection(string configXml)
            {
                var reader = new XmlTextReader(new StringReader(configXml));

                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                this.DeserializeSection(reader);

                // ReSharper restore DoNotCallOverridableMethodsInConstructor
            }

            #endregion
        }
    }
}
