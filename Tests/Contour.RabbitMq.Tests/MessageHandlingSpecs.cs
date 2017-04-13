using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Receiving;
using Contour.Transport.RabbitMq;
using Contour.Transport.RabbitMq.Topology;

using NUnit.Framework;

using Contour.Testing.Transport.RabbitMq;

namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The basic request response specs.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class MessageHandlingSpecs
    {
        /// <summary>
        /// The when_requesting_using_custom_callback_route.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_handling_request_message : RabbitMqFixture
        {
            /// <summary>
            /// The should_return_response.
            /// </summary>
            [Test]
            public void should_contain_reply_route()
            {
                IMessage message = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request").
                               WithCallbackEndpoint(
                                   b =>
                                       {
                                           Exchange e = b.Topology.Declare(
                                               Exchange.Named("dummy.response").AutoDelete);
                                           Queue q = b.Topology.Declare(
                                               Queue.Named("dummy.response").AutoDelete.Exclusive);

                                           b.Topology.Bind(e, q);

                                           return new SubscriptionEndpoint(q, e);
                                       }));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           message = ctx.Message;
                                           ctx.Reply(new DummyResponse(m.Num * 2));
                                       }));

                Task response = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13));

                response.Wait(3.Seconds()).
                    Should().
                    BeTrue();

                message.Headers[Headers.ReplyRoute].Should().NotBeNull();
                ((RabbitRoute)message.Headers[Headers.ReplyRoute]).Exchange.Should().Be("dummy.response");
                message.Headers[Headers.CorrelationId].Should().NotBeNull();
            }
        }

    }

    // ReSharper restore InconsistentNaming
}
