using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using FluentAssertions;

using Contour.Helpers;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;

using NUnit.Framework;
using FluentAssertions.Extensions;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The qo s publishing specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class QoSPublishingSpecs
    {
        /// <summary>
        /// The when_consuming_with_qos_set_to_prefetch_1.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_with_qos_set_to_prefetch_1 : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_consume_serialized.
            /// </summary>
            [Test]
            public void should_consume_serialized()
            {
                int result = 0;
                const int target = 5;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("dummy.request"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request").
                               WithQoS(new QoSParams(1, 0)).
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           Interlocked.Add(ref result, m.Num);
                                           if (result == target)
                                           {
                                               waitHandle.Set();
                                           }

                                           Thread.Sleep(1.Seconds());
                                       }));

                Enumerable.Range(0, target).
                    ForEach(_ => producer.Emit("dummy.request", new DummyRequest(2)));

                waitHandle.WaitOne(2.Seconds()).
                    Should().
                    BeFalse();
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
