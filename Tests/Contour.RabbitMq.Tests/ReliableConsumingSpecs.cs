using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The reliable consuming specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class ReliableConsumingSpecs
    {
        /// <summary>
        /// The given_producer_and_consumer_is_set.
        /// </summary>
        public class given_producer_and_consumer_is_set : RabbitMqFixture
        {
            #region Constants

            /// <summary>
            /// The total.
            /// </summary>
            public const int Total = 100;

            #endregion

            #region Fields

            /// <summary>
            /// The completion handler.
            /// </summary>
            public readonly AutoResetEvent CompletionHandler = new AutoResetEvent(false);

            /// <summary>
            /// The consumer.
            /// </summary>
            public IBus Consumer;

            /// <summary>
            /// The producer.
            /// </summary>
            public IBus Producer;

            /// <summary>
            /// The result.
            /// </summary>
            public bool[] Result = new bool[Total];

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// »нициализирует новый экземпл€р класса <see cref="given_producer_and_consumer_is_set"/>.
            /// </summary>
            /// <param name="acceptRequired">
            /// The accept required.
            /// </param>
            public given_producer_and_consumer_is_set(bool acceptRequired)
            {
                this.Producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.Consumer = this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            IReceiverConfigurator<BooMessage> receiverConfiguration = cfg.On<BooMessage>("boo").
                                ReactWith(
                                    (m, ctx) =>
                                        {
                                            this.Result[m.Num] = true;
                                            if (this.Result.All(r => r))
                                            {
                                                this.CompletionHandler.Set();
                                            }

                                            ctx.Accept();
                                        });

                            if (acceptRequired)
                            {
                                receiverConfiguration.RequiresAccept();
                            }
                        });
            }

            #endregion
        }

        /// <summary>
        /// The when_consuming_messages_on_thread_pool.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_messages_on_thread_pool : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_cause_channel_exception.
            /// </summary>
            [Test]
            public void should_not_cause_channel_exception()
            {
                var countdown = new CountdownEvent(10000);
                var random = new Randomizer();

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("request").
                               WithDefaultCallbackEndpoint());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("request").
                               ReactWith(
                                   (m, ctx) => Task.Factory.StartNew(
                                       () =>
                                           {
                                               Thread.Sleep(random.Next(10));
                                               ctx.Accept();
                                               ctx.Reply(new DummyResponse(m.Num));
                                               countdown.Signal();
                                           })).
                               RequiresAccept());

                Enumerable.Range(0, 10000).
                    ForEach(i => producer.RequestAsync<DummyRequest, DummyResponse>("request", new DummyRequest(i)));

                countdown.Wait(40.Seconds()).
                    Should().
                    BeTrue();
            }

            #endregion
        }

        /// <summary>
        /// The when_published_message_is_accepted_with_confirmation_enabled.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_published_message_is_accepted_with_confirmation_enabled : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_set_success_state.
            /// </summary>
            [Test]
            public void should_set_success_state()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo").
                               WithConfirmation());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").
                               ReactWith((m, ctx) => ctx.Accept()).
                               RequiresAccept());

                Task confirmation = producer.Emit("boo", new BooMessage(13));
                confirmation.Wait(5.Seconds()).
                    Should().
                    BeTrue();
                confirmation.IsFaulted.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_published_message_is_rejected_with_confirmation_enabled.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_published_message_is_rejected_with_confirmation_enabled : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_set_failure_state.
            /// </summary>
            [Test]
            [Ignore("Unable to simulate the case.")]
            public void should_set_failure_state()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request").
                               WithConfirmation());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request").
                               ReactWith((m, ctx) => ctx.Reject(false)).
                               RequiresAccept());

                Task confirmation = producer.Emit("dummy.request", new DummyRequest(13));
                confirmation.Wait(5.Seconds()).
                    Should().
                    BeTrue();
                confirmation.IsFaulted.Should().
                    BeTrue();
            }

            #endregion
        }

        /// <summary>
        /// The when_publishing_for_breaking_consumer_with_accept_required.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_for_breaking_consumer_with_accept_required : given_producer_and_consumer_is_set
        {
            #region Constructors and Destructors

            /// <summary>
            /// »нициализирует новый экземпл€р класса <see cref="when_publishing_for_breaking_consumer_with_accept_required"/>.
            /// </summary>
            public when_publishing_for_breaking_consumer_with_accept_required()
                : base(true)
            {
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The should_deliver_all_messages.
            /// </summary>
            [Test]
            public void should_deliver_all_messages()
            {
                var waitHandler = new AutoResetEvent(false);
                this.Consumer.Stopped += (bus, args) => waitHandler.Set();

                for (int i = 0; i < Total; i++)
                {
                    this.Producer.Emit("boo", new BooMessage(i));

                    if (i == Total / 3)
                    {
                        this.Consumer.Shutdown();
                        waitHandler.WaitOne();
                    }

                    if (i == (Total / 3) * 2)
                    {
                        this.Consumer.Start();
                    }
                }

                this.CompletionHandler.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();
            }

            #endregion
        }

        /// <summary>
        /// The when_publishing_for_breaking_consumer_without_accept_required.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_for_breaking_consumer_without_accept_required : given_producer_and_consumer_is_set
        {
            #region Constructors and Destructors

            /// <summary>
            /// »нициализирует новый экземпл€р класса <see cref="when_publishing_for_breaking_consumer_without_accept_required"/>.
            /// </summary>
            public when_publishing_for_breaking_consumer_without_accept_required()
                : base(false)
            {
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The should_not_deliver_some_messages.
            /// </summary>
            [Test]
            public void should_not_deliver_some_messages()
            {
                for (int i = 0; i < Total; i++)
                {
                    this.Producer.Emit("boo", new BooMessage(i));

                    if (i == Total / 3)
                    {
                        this.Consumer.Shutdown();
                        Thread.Sleep(300);
                    }

                    if (i == (Total / 3) * 2)
                    {
                        this.Consumer.Start();
                        Thread.Sleep(300);
                    }
                }

                this.CompletionHandler.WaitOne(5.Seconds()).
                    Should().
                    BeFalse();
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
