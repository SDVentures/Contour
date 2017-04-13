using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using FluentAssertions;

using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMq.Topology;

using NUnit.Framework;

using Queue = Contour.Transport.RabbitMq.Topology.Queue;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The unhandled publishing specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class UnhandledPublishingSpecs
    {
        /// <summary>
        /// The when_using_dead_letter_queue_unhandled_strategy.
        /// </summary>
        [TestFixture]
        [Ignore("При текущей конфигурации шины нельзя воспроизвести необработанное исключение.")]
        [Category("Integration")]
        public class when_using_dead_letter_queue_unhandled_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_move_messages_to_separate_queue.
            /// </summary>
            [Test]
            public void should_move_messages_to_separate_queue()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("foo")
                        .ConfiguredWith(rrb => rrb.Topology.Declare(Exchange.Named("all.in"))));

                bool wrongHandler = false;

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("all.dead")
                                .ConfiguredWith(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("all.dead"));
                                            Queue q = b.Topology.Declare(Queue.Named("all.dead"));
                                            b.Topology.Bind(e, q);
                                            return e;
                                        });

                            cfg.OnUnhandled(
                                d =>
                                    {
                                        d.Forward("all.dead", d.BuildFaultMessage());
                                        d.Accept();
                                    });

                            cfg.On<BooMessage>("boo")
                                .ReactWith((m, ctx) => { wrongHandler = true; })
                                .RequiresAccept()
                                .WithEndpoint(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("all.in"));
                                            Queue q = b.Topology.Declare(Queue.Named("all.in"));
                                            b.Topology.Bind(e, q);
                                            return new SubscriptionEndpoint(q, e);
                                        });
                        });

                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = 
                    this.Broker.GetMessages(this.VhostName, "all.in", 10, false);
                unackedMessages.Should().HaveCount(0);

                List<Testing.Plumbing.Message> deadMessages = 
                    this.Broker.GetMessages("integration", "all.dead", 10, false);
                deadMessages.Should().HaveCount(c => c > 0);

                wrongHandler.Should().BeFalse();
            }
        }

        /// <summary>
        /// The when_using_default_unhandled_strategy.
        /// </summary>
        [TestFixture]
        [Ignore("При текущей конфигурации шины нельзя воспроизвести необработанное исключение.")]
        [Category("Integration")]
        public class when_using_default_unhandled_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_silently_accept.
            /// </summary>
            [Test]
            public void should_silently_accept()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("foo")
                        .ConfiguredWith(rrb => rrb.Topology.Declare(Exchange.Named("all.in"))));

                bool wrongHandler = false;

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo")
                                .ReactWith(
                                    (m, ctx) =>
                                        {
                                            wrongHandler = true;
                                        })
                                .RequiresAccept()
                                .WithEndpoint(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("all.in"));
                                            Queue q = b.Topology.Declare(Queue.Named("all.in"));
                                            b.Topology.Bind(e, q);
                                            return new SubscriptionEndpoint(q, e);
                                        }));

                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = 
                    this.Broker.GetMessages(this.VhostName, "all.in", 10, false);
                unackedMessages.Should().HaveCount(0);

                wrongHandler.Should().BeFalse();
            }
        }

        /// <summary>
        /// The when_using_rejecting_unhandled_strategy.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        [Ignore("При текущей конфигурации шины нельзя воспроизвести необработанное исключение.")]
        public class when_using_rejecting_unhandled_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_keep_in_queue.
            /// </summary>
            [Test]
            public void should_keep_in_queue()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("foo")
                        .ConfiguredWith(rrb => rrb.Topology.Declare(Exchange.Named("all.in"))));

                bool wrongHandler = false;

                IBus consumer = this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.OnUnhandled(d => d.Reject(true));

                            cfg.On<BooMessage>("boo")
                                .ReactWith((m, ctx) => { wrongHandler = true; })
                                .RequiresAccept()
                                .WithEndpoint(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("all.in"));
                                            Queue q = b.Topology.Declare(Queue.Named("all.in"));
                                            b.Topology.Bind(e, q);
                                            return new SubscriptionEndpoint(q, e);
                                        });
                        });

                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));

                consumer.Shutdown();

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = 
                    this.Broker.GetMessages(this.VhostName, "all.in", 10, false);
                unackedMessages.Should().HaveCount(c => c > 0);

                wrongHandler.Should().BeFalse();
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
