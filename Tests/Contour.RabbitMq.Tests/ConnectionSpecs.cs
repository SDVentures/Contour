using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ.Internal;
using Contour.Transport.RabbitMQ.Topology;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client.Exceptions;

namespace Contour.RabbitMq.Tests
{
    using Moq;
    using Transport.RabbitMQ;

    // ReSharper disable InconsistentNaming
    
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public class ConnectionSpecs
    {
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_connection : RabbitMqFixture
        {
            [Test]
            public void should_not_close_connection_on_channel_failure()
            {
                var bus = this.ConfigureBus("Test", cfg => { });
                var tcs = new TaskCompletionSource<bool>(true);
                
                var connection = new RabbitConnection(new Endpoint("test"), this.ConnectionString, bus);
                connection.Closed += (sender, args) => tcs.SetResult(false);

                var tokenSource = new CancellationTokenSource();
                connection.Open(tokenSource.Token);
                var channel = connection.OpenChannel(tokenSource.Token);

                channel.Abort();
                Assert.Throws<AlreadyClosedException>(
                    () => channel.Bind(Queue.Named("q").Instance, Exchange.Named("e").Instance, "key"));
                
                Assert.True(!tcs.Task.IsCompleted);
            }
        }

        [TestFixture]
        [Category("Integration")]
        public class when_declaring_consumers_and_producers : RabbitMqFixture
        {
            [SetUp]
            public override void SetUp()
            {
                base.SetUp();
                this.Broker.DropConnections();
            }

            [Test]
            public void should_reuse_connection_in_each_consumer_if_reusable()
            {
                var bus = this.ConfigureBus(
                    "Test",
                    cfg =>
                    {
                        cfg.On<BooMessage>("one")
                            .ReactWith(m => { })
                            .WithEndpoint(
                                builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("one.queue"))))
                            .ReuseConnection();

                        cfg.On<FooMessage>("two")
                            .ReactWith(m => { })
                            .WithEndpoint(
                                builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("two.queue"))))
                            .ReuseConnection();

                        cfg.On<GooMessage>("three")
                            .ReactWith(m => { })
                            .WithEndpoint(
                                builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("three.queue"))))
                            .ReuseConnection();
                    });

                bus.Start();
                var cons = this.Broker.GetConnections();

                Assert.IsTrue(cons.Count() == 1);
            }

            [Test]
            public void should_create_separate_connection_for_each_consumer_if_not_reusable()
            {
                var bus = this.ConfigureBus(
                    "Test",
                    cfg =>
                    {
                        cfg.On<BooMessage>("one")
                            .ReactWith(m => { })
                            .WithEndpoint(builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("one.queue"))))
                            .ReuseConnection(false);

                        cfg.On<FooMessage>("two")
                            .ReactWith(m => { })
                            .WithEndpoint(builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("two.queue"))))
                            .ReuseConnection(false);

                        cfg.On<GooMessage>("three")
                            .ReactWith(m => { })
                            .WithEndpoint(builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("three.queue"))))
                            .ReuseConnection(false);
                    });

                bus.Start();
                var cons = this.Broker.GetConnections();
                
                // One connection is used by default by fault message producers
                Assert.IsTrue(cons.Count() == 3 + 1);
            }

            [Test]
            public void should_reuse_connection_in_each_producer_if_reusable()
            {
                var bus = this.ConfigureBus(
                   "Test",
                   cfg =>
                   {
                       cfg.Route("one.label").ReuseConnection();
                       cfg.Route("two.label").ReuseConnection();
                       cfg.Route("three.label").ReuseConnection();
                       cfg.Route("four.label").ReuseConnection();
                   });

                bus.Start();
                var cons = this.Broker.GetConnections();

                Assert.IsTrue(cons.Count() == 1);
            }

            [Test]
            public void should_create_separate_connection_for_each_producer_if_not_reusable()
            {
                var bus = this.ConfigureBus(
                    "Test",
                    cfg =>
                    {
                        cfg.Route("one.label")
                        .ReuseConnection(false);

                        cfg.Route("two.label")
                        .ReuseConnection(false);

                        cfg.Route("three.label")
                        .ReuseConnection(false);

                        cfg.Route("four.label")
                        .ReuseConnection(false);
                    });

                bus.Start();
                var cons = this.Broker.GetConnections();

                // One connection is used by default by fault message producers
                Assert.IsTrue(cons.Count() == 4 + 1);
            }
        }

        /// <summary>
        /// The when_connecting_to_invalid_broker_endpoint.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_connecting_to_invalid_broker_endpoint : RabbitMqFixture
        {
            /// <summary>
            /// The should_not_mark_bus_as_ready.
            /// </summary>
            [Test]
            public void should_not_mark_bus_as_ready()
            {
                IBus bus = this.ConfigureBus(
                    "Test", 
                    cfg =>
                        {
                            cfg.SetConnectionString("amqp://10.10.10.10/integration");

                            cfg.Route("some.label"); // just to pass validation
                        });

                bus.Start(false);

                bus.WhenReady.WaitOne(5.Seconds())
                    .Should()
                    .BeFalse();
            }
        }
        
        /// <summary>
        /// The when_restarting_bus_instance.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_restarting_bus_instance : RabbitMqFixture
        {
            /// <summary>
            /// The should_stop_and_start_without_resource_leakage.
            /// </summary>
            [Test]
            public void should_stop_and_start_without_resource_leakage()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();

                Task.Factory.StartNew(
                    () =>
                        {
                            Thread.Sleep(2.Seconds());

                            ((IBusAdvanced)bus).Panic();
                        });

                bus.WhenReady.WaitOne(5.Seconds()).Should().BeTrue();
            }
        }

        /// <summary>
        /// The when_sending_using_stopped_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_sending_using_stopped_bus : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Invoking(b => b.Emit("some.label", new BooMessage(666))).ShouldThrow<BusNotReadyException>();
            }
        }

        /// <summary>
        /// The when_starting_instances_concurrently.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_starting_instances_concurrently : RabbitMqFixture
        {
            /// <summary>
            /// The should_open_single_connection.
            /// </summary>
            [Test]
            [Explicit("Too much chaos.")]
            public void should_open_single_connection()
            {
                var random = new Randomizer();
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();

                Task[] tasks = Enumerable.Range(0, 20).Select(
                        _ =>
                            {
                                Thread.Sleep(random.Next(10));
                                if (random.Next(10) % 2 == 0)
                                {
                                    return Task.Factory.StartNew(() => bus.Start());
                                }

                                return Task.Factory.StartNew(bus.Stop);
                            }).ToArray();

                Task.WaitAll(tasks, 15.Seconds()).Should().BeTrue();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
