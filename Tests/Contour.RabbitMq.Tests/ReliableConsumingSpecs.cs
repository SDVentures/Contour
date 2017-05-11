using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;
using RabbitMQ.Client;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The reliable consuming specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "Reviewed. Suppression is OK here.")]
    public class ReliableConsumingSpecs
    {
        /// <summary>
        /// The given_producer_and_consumer_is_set.
        /// </summary>
        [TestFixture]
        public class given_producer_and_consumer_is_set : RabbitMqFixture
        {
            [SetUp]
            public override void SetUp()
            {
                base.SetUp();

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

                        if (this.acceptRequired)
                        {
                            receiverConfiguration.RequiresAccept();
                        }
                    });
            }

            /// <summary>
            /// The total.
            /// </summary>
            public const int Total = 100;

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

            protected bool acceptRequired;
        }

        /// <summary>
        /// The when_consuming_messages_on_thread_pool.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_messages_on_thread_pool : RabbitMqFixture
        {
            /// <summary>
            /// The should_not_cause_channel_exception.
            /// </summary>
            [Test]
            public void should_not_cause_channel_exception()
            {
                const int count = 100;
                var countdown = new CountdownEvent(count);
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

                Enumerable.Range(0, count).
                    ForEach(i => producer.RequestAsync<DummyRequest, DummyResponse>("request", new DummyRequest(i)));

                countdown.Wait(40.Seconds()).
                    Should().
                    BeTrue();
            }
        }

        /// <summary>
        /// The when_published_message_is_accepted_with_confirmation_enabled.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_published_message_is_accepted_with_confirmation_enabled : RabbitMqFixture
        {
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
        }

        /// <summary>
        /// The when_published_message_is_rejected_with_confirmation_enabled.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_published_message_is_rejected_with_confirmation_enabled : RabbitMqFixture
        {
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
        }

        /// <summary>
        /// The when_publishing_for_breaking_consumer_with_accept_required.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_for_breaking_consumer_with_accept_required : given_producer_and_consumer_is_set
        {
            [SetUp]
            public override void SetUp()
            {
                this.acceptRequired = true;
                base.SetUp();
            }

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
        }

        [TestFixture]
        [Category("Integration")]
        public class when_consuming_several_labels_in_one_endpoint : RabbitMqFixture
        {
            [Test]
            public void should_react_on_each_label()
            {
                const string Label1 = "label-1";
                const string Label2 = "label-2";

                var tcs1 = new TaskCompletionSource<bool>();
                var tcs2 = new TaskCompletionSource<bool>();

                var consumer = this.StartBus(
                    "consumer",
                    cfg =>
                    {
                        cfg
                            .On<DummyRequest>(Label1)
                            .ReactWith((m, ctx) =>
                            {
                                tcs1.SetResult(true);
                            });
                        cfg
                            .On<DummyRequest>(Label2)
                            .ReactWith((m, ctx) =>
                            {
                                tcs2.SetResult(true);
                            });
                    });

                consumer.WhenReady.WaitOne();

                var producer = this.StartBus("producer", cfg =>
                {
                    cfg.Route(Label1);
                    cfg.Route(Label2);
                });
                producer.WhenReady.WaitOne();

                producer.Emit(Label1, new DummyRequest(0));
                tcs1.Task.Wait(1.Minutes()).Should().BeTrue();

                producer.Emit(Label2, new DummyRequest(0));
                tcs2.Task.Wait(1.Minutes()).Should().BeTrue();
            }
        }

        [TestFixture]
        public class when_consuming_without_label : RabbitMqFixture
        {
            /// <summary>
            /// The default message routing suggests that there should be exactly one queue bound to the exchange and only one consumer attached to a queue.
            /// This fixture will ensure that a message without a label will be handled correctly in the only consumer attached.
            /// </summary>
            [Test]
            public void should_take_consumer_by_exchange()
            {
                const string Label1 = "command.handle.this-1";
                const string Label2 = "command.handle.this-2";

                var tcs1 = new TaskCompletionSource<bool>();
                var tcs2 = new TaskCompletionSource<bool>();

                var bus = this.StartBus(
                    "consumer",
                    cfg =>
                    {
                        cfg.On(Label1).ReactWith<FooMessage>(
                        (m, ctx) =>
                        {
                            tcs1.SetResult(true);
                        });

                        cfg.On(Label2).ReactWith<FooMessage>(
                        (m, ctx) =>
                        {
                            tcs2.SetResult(true);
                        });
                    });

                bus.WhenReady.WaitOne(1.Minutes()).Should().BeTrue();

                var factory = new ConnectionFactory() { Uri = this.ConnectionString };
                var connection = factory.CreateConnection();
                var publishModel = connection.CreateModel();

                var content = Encoding.Default.GetBytes("{}");
                var properties = publishModel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>();

                publishModel.BasicPublish(Label2, string.Empty, properties, content);
                tcs2.Task.Wait(1.Minutes()).Should().BeTrue();

                publishModel.BasicPublish(Label1, string.Empty, properties, content);
                tcs1.Task.Wait(1.Minutes()).Should().BeTrue();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
