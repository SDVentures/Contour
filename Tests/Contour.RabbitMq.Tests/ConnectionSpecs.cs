using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Configuration;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ.Internal;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The connection specs.
    /// </summary>
    public class ConnectionSpecs
    {
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
        /// The when_connecting_to_valid_broker_endpoint.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_connecting_to_valid_broker_endpoint : RabbitMqFixture
        {
            /// <summary>
            /// The should_successfully_connect.
            /// </summary>
            [Test]
            public void should_successfully_connect()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                var waitHandler = new AutoResetEvent(false);

                bus.Connected += (b, args) => waitHandler.Set();

                bus.Invoking(b => b.Start())
                    .ShouldNotThrow<BusConnectionException>();

                waitHandler.WaitOne(2.Seconds())
                    .Should()
                    .BeTrue();
            }
        }

        /// <summary>
        /// The when_connection_is_failed.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_connection_is_failed : RabbitMqFixture
        {
            /// <summary>
            /// The should_try_to_connect.
            /// </summary>
            [Test]
            public void should_try_to_connect()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bool disconnectedIsRaised = false;
                bus.Disconnected += (bus1, args) => { disconnectedIsRaised = true; };

                bus.Start();

                Task.Factory.StartNew(
                    () =>
                        {
                            Thread.Sleep(2.Seconds());

                            ((RabbitBus)bus).Connection.Abort();
                        });

                int counter = 10;
                while (counter-- > 0)
                {
                    try
                    {
                        bus.Emit("some.label", new BooMessage(666));
                        Thread.Sleep(1.Seconds());
                    }
                    catch (Exception ex)
                    {
                        ex.Should().BeOfType<BusNotReadyException>();
                    }
                }

                disconnectedIsRaised.Should().BeTrue();
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
