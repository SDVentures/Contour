using System.Collections.Generic;
using System.IO;
using System.Threading;

using FluentAssertions;

using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ.Topology;

using NUnit.Framework;

using Queue = Contour.Transport.RabbitMQ.Topology.Queue;

namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The failed publishing specs.
    /// </summary>
    public class FailedPublishingSpecs
    {
        /// <summary>
        /// The when_encountering_serialization_error.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_encountering_serialization_error : RabbitMqFixture
        {
            /// <summary>
            /// The should_use_failure_handler.
            /// </summary>
            [Test]
            public void should_use_failure_handler()
            {
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<WrongBooMessage>("boo")
                        .ReactWith(
                            (m, ctx) =>
                                {
                                })
                        .RequiresAccept()
                        .OnFailed(
                                   d =>
                                       {
                                           waitHandle.Set();
                                           d.Accept();
                                       }));

                producer.Emit("boo", new FooMessage(13));

                waitHandle.WaitOne(5.Seconds()).Should().BeTrue();
            }
        }

        /// <summary>
        /// The when_using_default_failure_strategy.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_using_default_failure_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_silently_accept.
            /// </summary>
            /// <exception cref="IOException">
            /// </exception>
            [Test]
            public void should_silently_accept()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo")
                        .ReactWith(
                            (m, ctx) =>
                                {
                                    throw new IOException();
                                })
                        .RequiresAccept());

                producer.Emit("boo", new FooMessage(13));
                producer.Emit("boo", new FooMessage(13));
                producer.Emit("boo", new FooMessage(13));

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = this.Broker.GetMessages(this.VhostName, "Receiver.boo", 10, false);
                unackedMessages.Should().HaveCount(0);
            }
        }

        /// <summary>
        /// The when_using_invalid_message_queue_failure_strategy.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_using_invalid_message_queue_failure_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_move_messages_to_separate_queue.
            /// </summary>
            /// <exception cref="IOException">
            /// </exception>
            [Test]
            public void should_move_messages_to_separate_queue()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("all.broken")
                                .ConfiguredWith(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("all.broken"));
                                            Queue q = b.Topology.Declare(Queue.Named("all.broken"));
                                            b.Topology.Bind(e, q);
                                            return e;
                                        });
                            cfg.OnFailed(
                                d =>
                                    {
                                        d.Forward("all.broken", d.BuildFaultMessage());
                                        d.Accept();
                                    });

                            cfg.On<BooMessage>("boo")
                                .ReactWith((m, ctx) => { throw new IOException(); })
                                .RequiresAccept();
                        });

                producer.Emit("boo", new FooMessage(13));
                producer.Emit("boo", new FooMessage(13));
                producer.Emit("boo", new FooMessage(13));

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = this.Broker.GetMessages(this.VhostName, "Receiver.boo", 10, false);
                unackedMessages.Should().HaveCount(0);

                List<Testing.Plumbing.Message> brokenMessages = this.Broker.GetMessages(this.VhostName, "all.broken", 10, false);
                brokenMessages.Should().HaveCount(c => c > 0);
            }
        }

        /// <summary>
        /// The when_using_separate_invalid_message_queues_failure_strategy.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_using_separate_invalid_message_queues_failure_strategy : RabbitMqFixture
        {
            /// <summary>
            /// The should_move_messages_to_separate_queues.
            /// </summary>
            /// <exception cref="IOException">
            /// </exception>
            [Test]
            public void should_move_messages_to_separate_queues()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("boo");
                            cfg.Route("foo");
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("boo.broken")
                                .ConfiguredWith(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("boo.broken"));
                                            Queue q = b.Topology.Declare(Queue.Named("boo.broken"));
                                            b.Topology.Bind(e, q);
                                            return e;
                                        });

                            cfg.Route("foo.broken")
                                .ConfiguredWith(
                                    b =>
                                        {
                                            Exchange e = b.Topology.Declare(Exchange.Named("foo.broken"));
                                            Queue q = b.Topology.Declare(Queue.Named("foo.broken"));
                                            b.Topology.Bind(e, q);
                                            return e;
                                        });

                            cfg.On<BooMessage>("boo")
                                .ReactWith((m, ctx) => { throw new IOException(); })
                                .RequiresAccept()
                                .OnFailed(
                                    d =>
                                        {
                                            d.Forward("boo.broken", d.BuildFaultMessage());
                                            d.Accept();
                                        });

                            cfg.On<BooMessage>("foo")
                                .ReactWith((m, ctx) => { throw new IOException(); })
                                .RequiresAccept()
                                .OnFailed(
                                    d =>
                                        {
                                            d.Forward("foo.broken", d.BuildFaultMessage());
                                            d.Accept();
                                        });
                        });

                producer.Emit("boo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));
                producer.Emit("boo", new FooMessage(13));
                producer.Emit("foo", new FooMessage(13));

                Thread.Sleep(500);

                List<Testing.Plumbing.Message> unackedMessages = this.Broker.GetMessages(this.VhostName, "Receiver.boo", 10, false);
                unackedMessages.Should().HaveCount(0);

                unackedMessages = this.Broker.GetMessages(this.VhostName, "Receiver.foo", 10, false);
                unackedMessages.Should().HaveCount(0);

                List<Testing.Plumbing.Message> brokenMessages = this.Broker.GetMessages(this.VhostName, "boo.broken", 10, false);
                brokenMessages.Should().HaveCount(c => c > 0);

                brokenMessages = this.Broker.GetMessages(this.VhostName, "foo.broken", 10, false);
                brokenMessages.Should().HaveCount(c => c > 0);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
