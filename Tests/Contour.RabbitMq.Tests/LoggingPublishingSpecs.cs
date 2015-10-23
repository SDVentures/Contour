using System;
using System.Dynamic;
using System.Threading;

using FluentAssertions;

using Contour.Receiving;
using Contour.Sending;
using Contour.Transport.RabbitMQ.Topology;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The logging publishing specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class LoggingPublishingSpecs
    {
        /// <summary>
        /// The when_publishing_simple_message_with_configured_logger_present.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_simple_message_with_configured_logger_present : RabbitMqFixture
        {
            /// <summary>
            /// The should_catch_message_on_both_consumer_and_logger.
            /// </summary>
            [Test]
            public void should_catch_message_on_both_consumer_and_logger()
            {
                var consumerWaitHandle = new AutoResetEvent(false);
                var loggerWaitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request")
                        .ConfiguredWith(b => b.Topology.Declare(Exchange.Named("dummy.request").Durable)));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith(_ => consumerWaitHandle.Set()).WithEndpoint(
                            b =>
                                {
                                    Exchange e = b.Topology.Declare(Exchange.Named("dummy.request").Durable);
                                    Queue q = b.Topology.Declare(Queue.Named("dummy.request").Durable);
                                    b.Topology.Bind(e, q);

                                    return new SubscriptionEndpoint(q);
                                }));

                this.StartBus(
                    "Logger", 
                    cfg => cfg.On(MessageLabel.Any)
                        .ReactWith<ExpandoObject>(_ => loggerWaitHandle.Set())
                        .WithEndpoint(
                            b =>
                                {
                                    Exchange e = b.Topology.Declare(Exchange.Named("dummy.request").Durable);
                                    Queue q = b.Topology.Declare(Queue.Named("logger.input").Durable);
                                    b.Topology.Bind(e, q);

                                    return new SubscriptionEndpoint(q);
                                }));

                producer.Emit("dummy.request", new DummyRequest(13), new PublishingOptions { Persistently = true, Ttl = TimeSpan.FromSeconds(5) });

                consumerWaitHandle.WaitOne(5.Seconds()).Should().BeTrue();
                loggerWaitHandle.WaitOne(5.Seconds()).Should().BeTrue();
            }
        }

        /// <summary>
        /// Проверяет случай, когда отправляется сообщение с установленным TTL
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [TestFixture(Description = "Когда отправляется сообщение с установленным TTL")]
        [Category("Integration")]
        public class when_publishing_message_with_ttl : RabbitMqFixture
        {
            /// <summary>
            /// Тогда сообщение должно существовать указанный промежуток времени.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
            [Test]
            public void should_message_exist_during_specifeid_interval()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route("dummy.request"));
                this.Broker.CreateQueue(this.VhostName, "dummy.request");
                this.Broker.CreateBind(this.VhostName, "dummy.request", "dummy.request");

                producer.Emit("dummy.request", new { }, new PublishingOptions { Ttl = TimeSpan.FromSeconds(5) });

                Thread.Sleep(2000);
                var notEmptyMessages = this.Broker.GetMessages(this.VhostName, "dummy.request", int.MaxValue, false);
                Thread.Sleep(5000);
                var emptyMessages = this.Broker.GetMessages(this.VhostName, "dummy.request", int.MaxValue, false);

                Assert.IsNotEmpty(notEmptyMessages, "Должно быть сообщение.");
                Assert.AreEqual(1, notEmptyMessages.Count, "Должно быть сообщение.");

                Assert.IsEmpty(emptyMessages, "Должно быть удалено сообщение.");
                Assert.AreEqual(1, notEmptyMessages.Count, "Должно быть удаленосообщение.");
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
