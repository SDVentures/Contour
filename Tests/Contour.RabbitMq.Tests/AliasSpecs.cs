using System.Diagnostics.CodeAnalysis;
using System.Threading;

using FluentAssertions;

using Contour.Configuration;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The alias specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class AliasSpecs
    {
        /// <summary>
        /// The when_forwarding_message_using_existing_alias.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_forwarding_message_using_existing_alias : RabbitMqFixture
        {
            /// <summary>
            /// The should_catch_message_on_subscribed_consumer.
            /// </summary>
            [Test]
            public void should_catch_message_on_subscribed_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("boo.message").WithAlias("msg");
                            cfg.On<BooMessage>("boo.message.fwd")
                                .ReactWith(
                                    m =>
                                        {
                                            result = m.Num;
                                            waitHandle.Set();
                                        });
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("boo.message.fwd").WithAlias("fwd");
                            cfg.On<BooMessage>("boo.message")
                                .ReactWith((m, ctx) => ctx.Forward(":fwd"));
                        });

                producer.Emit(":msg", new BooMessage(13));

                waitHandle.WaitOne(5.Seconds())
                    .Should()
                    .BeTrue();

                result.Should().Be(13);
            }
        }

        /// <summary>
        /// The when_publishing_message_using_existing_alias.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_message_using_existing_alias : RabbitMqFixture
        {
            /// <summary>
            /// The should_catch_message_on_subscribed_consumer.
            /// </summary>
            [Test]
            public void should_catch_message_on_subscribed_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "procuder",
                    cfg => cfg.Route("boo.message").WithAlias("msg"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo.message")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = m.Num;
                                           waitHandle.Set();
                                       }));

                producer.Emit(":msg", new BooMessage(13));

                waitHandle.WaitOne(5.Seconds())
                    .Should()
                    .BeTrue();

                result.Should().Be(13);
            }
        }

        /// <summary>
        /// The when_publishing_message_using_non_existing_alias.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_message_using_non_existing_alias : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo.message").WithAlias("msg"));

                producer.Invoking(p => p.Emit(":msgz", new BooMessage(13)))
                    .ShouldThrow<BusConfigurationException>();
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
