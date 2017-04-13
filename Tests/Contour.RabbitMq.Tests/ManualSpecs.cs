using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Contour.Testing.Transport.RabbitMq;
using FluentAssertions;
using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The manual specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class ManualSpecs
    {
        /// <summary>
        /// The when_publishing_message_on_restarted_service.
        /// </summary>
        [TestFixture]
        [Category("Manual")]
        public class when_publishing_message_on_restarted_service : RabbitMqFixture
        {
            /// <summary>
            /// The should_reconnect_within_5_seconds.
            /// </summary>
            [Test]
            [Explicit("Manual run required")]
            public void should_reconnect_within_5_seconds()
            {
                const int total = 100;
                var countdown = new CountdownEvent(total);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                IBus consumer = this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           Console.WriteLine("Received {0}.", m.Num);
                                           countdown.Signal();
                                       }));

                int count = total;
                while (count -- > 0)
                {
                    producer.Emit("boo", new BooMessage(count));
                    Console.WriteLine("Sent {0}.", count);
                    Thread.Sleep(1.Seconds());
                }

                countdown.Wait();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
