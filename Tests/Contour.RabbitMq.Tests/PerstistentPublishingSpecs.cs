using System.Diagnostics.CodeAnalysis;
using System.Threading;

using FluentAssertions;

using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMq.Topology;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The perstistent publishing specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class PerstistentPublishingSpecs
    {
        /// <summary>
        /// The when_publishing_simple_message_persistently.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_simple_message_persistently : RabbitMqFixture
        {
            /// <summary>
            /// The should_persist_message.
            /// </summary>
            [Test]
            [Ignore("Broker weird issue")]
            public void should_persist_message()
            {
                var waitHandle = new CountdownEvent(3);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo").
                               Persistently().
                               ConfiguredWith(
                                   b => b.Topology.Declare(
                                       Exchange.Named("boo").
                                            Durable)));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           ctx.Reject(true);
                                           waitHandle.Signal();
                                       }).
                               RequiresAccept().
                               WithEndpoint(
                                   b =>
                                       {
                                           Exchange e = b.Topology.Declare(
                                               Exchange.Named("boo").
                                                   Durable);
                                           Queue q = b.Topology.Declare(
                                               Queue.Named("boo").
                                                   Durable);
                                           b.Topology.Bind(e, q);
                                           return new SubscriptionEndpoint(q, e);
                                       }));

                producer.Emit("boo", new BooMessage(13));
                producer.Emit("boo", new BooMessage(13));
                producer.Emit("boo", new BooMessage(13));

                waitHandle.Wait(5.Seconds()).
                    Should().
                    BeTrue();

                // TODO: restsharp somehow makes request which returns not all data
                var messages = this.Broker.GetMessages(this.VhostName, "boo", int.MaxValue, false);
                messages.Count.Should().Be(3);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
