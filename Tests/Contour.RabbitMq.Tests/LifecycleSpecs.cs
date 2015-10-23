using System.Threading;

using Contour.Testing.Transport.RabbitMq;

using FluentAssertions;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// The lifecycle specs.
    /// </summary>
    public class LifecycleSpecs
    {
        /// <summary>
        /// The when_building_without_starting.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_building_without_starting : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_attempt_to_connect.
            /// </summary>
            [Test]
            public void should_not_attempt_to_connect()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                var waitHandler = new AutoResetEvent(false);

                bus.Connected += (b, args) => waitHandler.Set();

                waitHandler.WaitOne(2.Seconds()).
                    Should().
                    BeFalse();
            }

            /// <summary>
            /// The should_not_be_ready.
            /// </summary>
            [Test]
            public void should_not_be_ready()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.WhenReady.WaitOne(2.Seconds()).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_shutting_down_non_started_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_shutting_down_non_started_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_throw.
            /// </summary>
            [Test]
            public void should_not_throw()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Invoking(b => b.Shutdown()).
                    ShouldNotThrow();

                bus.WhenReady.WaitOne(0).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_shutting_down_started_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_shutting_down_started_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_shut_down_without_throwing.
            /// </summary>
            [Test]
            public void should_shut_down_without_throwing()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();

                bus.Invoking(b => b.Shutdown()).
                    ShouldNotThrow();
                bus.WhenReady.WaitOne(0).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_shutting_down_starting_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_shutting_down_starting_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_shut_down_without_throwing.
            /// </summary>
            [Test]
            public void should_shut_down_without_throwing()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start(false);

                bus.Invoking(b => b.Shutdown()).
                    ShouldNotThrow();
                bus.WhenReady.WaitOne(0).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_shutting_down_stopped_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_shutting_down_stopped_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_throw.
            /// </summary>
            [Test]
            public void should_not_throw()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();
                bus.Stop();

                bus.Invoking(b => b.Shutdown()).
                    ShouldNotThrow();
                bus.WhenReady.WaitOne(0).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();

                bus.Invoking(b => b.Emit("some.label", new { })).
                    ShouldThrow<BusNotReadyException>();
            }

            #endregion
        }

        /// <summary>
        /// The when_starting_bus_after_shutdown.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_starting_bus_after_shutdown : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_start_normally.
            /// </summary>
            [Test]
            public void should_start_normally()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();
                bus.Shutdown();

                bus.Invoking(b => b.Start()).
                    ShouldNotThrow();
                bus.WhenReady.WaitOne(0).
                    Should().
                    BeTrue();
                bus.IsStarted.Should().
                    BeTrue();

                bus.Invoking(b => b.Emit("some.label", new { })).
                    ShouldNotThrow();
            }

            #endregion
        }

        /// <summary>
        /// The when_starting_bus_after_stop.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_starting_bus_after_stop : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_start_normally.
            /// </summary>
            [Test]
            public void should_start_normally()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();
                bus.Stop();

                bus.Invoking(b => b.Start()).
                    ShouldNotThrow();
                bus.WhenReady.WaitOne(0).
                    Should().
                    BeTrue();
                bus.IsStarted.Should().
                    BeTrue();

                bus.Invoking(b => b.Emit("some.label", new { })).
                    ShouldNotThrow();
            }

            #endregion
        }

        /// <summary>
        /// The when_starting_with_waiting.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_starting_with_waiting : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_be_ready.
            /// </summary>
            [Test]
            public void should_be_ready()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start();

                bus.WhenReady.WaitOne(0).
                    Should().
                    BeTrue();
                bus.IsStarted.Should().
                    BeTrue();
            }

            #endregion
        }

        /// <summary>
        /// The when_starting_without_waiting.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_starting_without_waiting : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_be_ready.
            /// </summary>
            [Test]
            public void should_not_be_ready()
            {
                IBus bus = this.ConfigureBus("Test", cfg => cfg.Route("some.label"));

                bus.Start(false);

                bus.WhenReady.WaitOne(0).
                    Should().
                    BeFalse();
                bus.IsStarted.Should().
                    BeFalse();
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
