namespace Contour.Common.Tests
{
    using System;
    using System.Threading;

    using FluentAssertions;

    using Contour.Helpers.Timing;

    using NUnit.Framework;

    /// <summary>
    /// The rough ticket timer specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class RoughTicketTimerSpecs
    {
        /// <summary>
        /// The when_callback_misbehaves.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_callback_misbehaves
        {
            /// <summary>
            /// The should_maintain_valid_ticket_dictionary.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// </exception>
            [Test]
            public void should_maintain_valid_ticket_dictionary()
            {
                var timer = new RoughTicketTimer(TimeSpan.FromSeconds(0.5));
                timer.Acquire(TimeSpan.FromSeconds(0.1), () => { throw new InvalidOperationException(); });
                timer.Acquire(TimeSpan.FromSeconds(0.2), () => { });

                Thread.Sleep(1000);

                timer.JobCount.Should().Be(0);
            }
        }

        /// <summary>
        /// The when_cancelling_using_valid_ticket.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_cancelling_using_valid_ticket
        {
            /// <summary>
            /// The should_cancel_scheduled_callback.
            /// </summary>
            [Test]
            public void should_cancel_scheduled_callback()
            {
                int result = 0;

                var timer = new RoughTicketTimer(TimeSpan.FromSeconds(1));
                timer.Acquire(TimeSpan.FromSeconds(0.4), () => Interlocked.Increment(ref result));
                timer.Acquire(TimeSpan.FromSeconds(0.8), () => Interlocked.Increment(ref result));
                long ticket = timer.Acquire(TimeSpan.FromSeconds(1.2), () => Interlocked.Increment(ref result));
                timer.Acquire(TimeSpan.FromSeconds(1.6), () => Interlocked.Increment(ref result));

                result.Should().Be(0);

                Thread.Sleep(600);
                result.Should().Be(0);

                timer.Cancel(ticket);

                Thread.Sleep(600);
                result.Should().Be(2);

                Thread.Sleep(600);
                result.Should().Be(2);

                Thread.Sleep(600);
                result.Should().Be(3);

                timer.JobCount.Should().Be(0);
            }
        }

        /// <summary>
        /// The when_waiting_for_callbacks.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_waiting_for_callbacks
        {
            /// <summary>
            /// The should_use_minimum_interval.
            /// </summary>
            [Test]
            public void should_use_minimum_interval()
            {
                int result = 0;

                var timer = new RoughTicketTimer(TimeSpan.FromSeconds(1));
                timer.Acquire(TimeSpan.FromSeconds(0.4), () => Interlocked.Increment(ref result));
                timer.Acquire(TimeSpan.FromSeconds(0.8), () => Interlocked.Increment(ref result));
                timer.Acquire(TimeSpan.FromSeconds(1.2), () => Interlocked.Increment(ref result));
                timer.Acquire(TimeSpan.FromSeconds(1.6), () => Interlocked.Increment(ref result));

                result.Should().Be(0);

                Thread.Sleep(600);
                result.Should().Be(0);

                Thread.Sleep(600);
                result.Should().Be(2);

                Thread.Sleep(600);
                result.Should().Be(2);

                Thread.Sleep(600);
                result.Should().Be(4);

                timer.JobCount.Should().Be(0);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
