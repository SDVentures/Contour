using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using FluentAssertions;

using Contour.Configuration;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ.Topology;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The bus configuration specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification =
        "Reviewed. Suppression is OK here.")]
    public class BusConfigurationSpecs
    {
        /// <summary>
        /// The when_building_configuration_without_consumers_and_producers.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_building_configuration_without_consumers_and_producers : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw_on_validation.
            /// </summary>
            [Test]
            [Ignore("Doesn't work considering we're using default dead letter queue.")]
            public void should_throw_on_validation()
            {
                Action busInitializationAction = () => this.StartBus("Test", cfg => { });

                busInitializationAction.ShouldThrow<BusConfigurationException>();
            }
        }

        /// <summary>
        /// The when_building_configuration_without_endpoint_set.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_building_configuration_without_endpoint_set : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw_on_validation.
            /// </summary>
            [Test]
            public void should_throw_on_validation()
            {
                IBus bus = null;
                Action busInitializationAction = () =>
                {
                    bus = new BusFactory().Create(cfg => cfg.Route("something"));
                };

                busInitializationAction.ShouldThrow<BusConfigurationException>();
                if (bus != null)
                {
                    bus.Shutdown();
                }
            }
        }

        /// <summary>
        /// The when_defining_consumer_using_lambda.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_defining_consumer_using_lambda : RabbitMqFixture
        {
            /// <summary>
            /// The should_build_valid_configuration.
            /// </summary>
            [Test]
            public void should_build_valid_configuration()
            {
                IBus bus = this.StartBus(
                    "Test",
                    cfg =>
                    {
                        cfg.On<BooMessage>("boo").ReactWith(m => { });
                        cfg.Route("foo").WithConfirmation();
                    });

                bus.CanHandle("boo").Should().BeTrue();
                bus.CanRoute("foo").Should().BeTrue();

                bus.CanHandle("foo").Should().BeFalse();
                bus.CanRoute("voo").Should().BeFalse();
            }
        }

        /// <summary>
        /// The when_defining_receivers_on_same_queue_with_different_accept_requirements.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_defining_receivers_on_same_queue_with_different_accept_requirements : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                IBus bus = this.ConfigureBus(
                    "Test",
                    cfg =>
                    {
                        cfg.On<BooMessage>("boo")
                            .ReactWith(m => { })
                            .WithEndpoint(seb => seb.ListenTo(seb.Topology.Declare(Queue.Named("some.queue"))))
                            .RequiresAccept();
                        cfg.On<FooMessage>("foo")
                            .ReactWith(m => { })
                            .WithEndpoint(seb => seb.ListenTo(seb.Topology.Declare(Queue.Named("some.queue"))));
                    });

                bus.Invoking(b => b.Start()).ShouldThrow<BusConfigurationException>();
            }
        }

        /// <summary>
        /// При создании очереди с <c>ttl</c>.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_declaring_queue_with_ttl : RabbitMqFixture
        {
            /// <summary>
            /// Сообщения должны удаляться по истечении времени жизни.
            /// </summary>
            [Test]
            public void should_remove_message_from_queue()
            {
                TimeSpan ttl = TimeSpan.FromSeconds(2);

                IBus bus = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo")
                        .ConfiguredWith(
                            b =>
                            {
                                Exchange e = b.Topology.Declare(Exchange.Named("boo").Fanout);
                                Queue q = b.Topology.Declare(Queue.Named("boo").WithTtl(ttl));
                                b.Topology.Bind(e, q);
                                return e;
                            }));

                bus.Emit("boo", new { });
                Thread.Sleep(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 2));

                var emptyMessages = this.Broker.GetMessages(this.VhostName, "boo", Int32.MaxValue, false);

                Assert.IsEmpty(emptyMessages, "Должно быть удалено сообщение.");
            }

            /// <summary>
            /// Сообщения должны оставаться в очереди до истечения времени жизни.
            /// </summary>
            [Test]
            public void should_stay_message_in_queue()
            {
                TimeSpan ttl = TimeSpan.FromSeconds(10);

                IBus bus = this.StartBus(
                    "producer",
                    cfg =>
                        cfg.Route("boo2")
                            .ConfiguredWith(
                                b =>
                                {
                                    Exchange e = b.Topology.Declare(Exchange.Named("boo2").Fanout);
                                    Queue q = b.Topology.Declare(Queue.Named("boo2").WithTtl(ttl));
                                    b.Topology.Bind(e, q);
                                    return e;
                                }));

                bus.WhenReady.WaitOne();
                bus.Emit("boo2", new { });

                Thread.Sleep(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds / 2));

                var notEmptyMessages = this.Broker.GetMessages(this.VhostName, "boo2", Int32.MaxValue, false);

                Assert.IsNotEmpty(notEmptyMessages, "Сообщения должны быть в очереди.");
            }
        }


        /// <summary>
        /// The when_configuring_producer_with_empty_label.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_configuring_producer_with_empty_label : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                var factory = new BusFactory();

                factory.Invoking(b => b.Create(cfg => cfg.Route(MessageLabel.Empty)))
                    .ShouldThrow<BusConfigurationException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_configuring_message_header_storage
        {
            [Test]
            public void should_set_message_header_storage_for_incoming_and_outgoing_messages_by_default()
            {
                var configuration = new BusConfiguration();
                var storage = new MessageHeaderStorage(Enumerable.Empty<string>());
                configuration.UseIncomingMessageHeaderStorage(storage);

                var senderStorage = configuration.SenderDefaults.GetIncomingMessageHeaderStorage();
                senderStorage.HasValue.Should().BeTrue();
                senderStorage.Value.Should().Be(storage);

                var receiverStorage = configuration.ReceiverDefaults.GetIncomingMessageHeaderStorage();
                receiverStorage.HasValue.Should().BeTrue();
                receiverStorage.Value.Should().Be(storage);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
