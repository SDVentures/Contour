using System.Diagnostics;
using System.Linq;

using Contour.Helpers;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;

namespace Contour.RabbitMq.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Contour.Configuration;
    using Contour.Receiving;
    using Contour.Sending;
    using Contour.Transport.RabbitMQ.Topology;

    using NUnit.Framework;

    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The basic request response specs.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class BasicRequestResponseSpecs
    {
        /// <summary>
        /// The when_global_timeout_is_overrided.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_global_timeout_is_overrided : RabbitMqFixture
        {
            /// <summary>
            /// The should_use_overrided_timeout_value.
            /// </summary>
            [Test]
            public void should_use_overrided_timeout_value()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request")
                        .WithRequestTimeout(TimeSpan.FromSeconds(1))
                        .WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           Thread.Sleep(TimeSpan.FromSeconds(3));
                                           ctx.Reply(new DummyResponse(m.Num));
                                       }));

                Task<DummyResponse> result = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13), new RequestOptions { Timeout = TimeSpan.FromSeconds(5) });

                Exception responseException = null;
                result.ContinueWith(
                    t =>
                        {
                            if (t.IsFaulted && t.Exception != null)
                            {
                                responseException = t.Exception.Flatten();
                            }
                        }).Wait();

                responseException.Should().BeNull();
            }
        }

        /// <summary>
        /// The when_replying_to_message_without_reply_address.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_replying_to_message_without_reply_address : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw_bus_configuration_exception.
            /// </summary>
            [Test]
            public void should_throw_bus_configuration_exception()
            {
                Exception exception = null;
                var failed = new ManualResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request"));
                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.OnFailed(
                                ctx =>
                                    {
                                        exception = ctx.Exception;
                                        ctx.Accept();
                                        failed.Set();
                                    });

                            cfg.On<DummyRequest>("dummy.request")
                                .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num * 2)));
                        });

                producer.Emit("dummy.request", new DummyRequest(13));

                failed.WaitOne(3.Seconds()).Should().BeTrue();
                exception.Should().BeOfType<BusConfigurationException>();
            }
        }

        /// <summary>
        /// The when_request_is_expired.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_request_is_expired : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw_timeout_exception.
            /// </summary>
            [Test]
            public void should_throw_timeout_exception()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request").WithDefaultCallbackEndpoint());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           Thread.Sleep(TimeSpan.FromSeconds(5));
                                           ctx.Reply(new DummyResponse(m.Num));
                                       }));

                Task<DummyResponse> result = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13), new RequestOptions { Timeout = TimeSpan.FromSeconds(1) });

                Exception responseException = null;
                result.ContinueWith(
                    t =>
                        {
                            if (t.IsFaulted && t.Exception != null)
                            {
                                responseException = t.Exception.Flatten();
                            }
                        }).Wait();

                responseException.Should().NotBeNull();
                responseException.InnerException.Should().BeOfType<TimeoutException>();
            }
        }

        /// <summary>
        /// The when_request_is_expired_with_global_timeout_set.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_request_is_expired_with_global_timeout_set : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw_timeout_exception.
            /// </summary>
            [Test]
            public void should_throw_timeout_exception()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request").WithRequestTimeout(TimeSpan.FromSeconds(1)).WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           Thread.Sleep(TimeSpan.FromSeconds(5));
                                           ctx.Reply(new DummyResponse(m.Num));
                                       }));

                Task<DummyResponse> result = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13));

                Exception responseException = null;
                result.ContinueWith(
                    t =>
                        {
                            if (t.IsFaulted && t.Exception != null)
                            {
                                responseException = t.Exception.Flatten();
                            }
                        }).Wait();

                responseException.Should().NotBeNull();
                responseException.InnerException.Should().BeOfType<TimeoutException>();
            }
        }

        /// <summary>
        /// The when_requesting_using_custom_callback_route.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_requesting_using_custom_callback_route : RabbitMqFixture
        {
            /// <summary>
            /// The should_return_response.
            /// </summary>
            [Test]
            public void should_return_response()
            {
                int result = 0;

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request")
                        .WithCallbackEndpoint(
                                   b =>
                                       {
                                           Exchange e = b.Topology.Declare(Exchange.Named("dummy.response").AutoDelete);
                                           Queue q = b.Topology.Declare(Queue.Named("dummy.response").AutoDelete.Exclusive);

                                           b.Topology.Bind(e, q);

                                           return new SubscriptionEndpoint(q, e);
                                       }));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num * 2))));

                Task<int> response = producer
                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13))
                    .ContinueWith(t => result = t.Result.Num);

                response.Wait(3.Seconds()).Should().BeTrue();
                result.Should().Be(26);
            }
        }

        /// <summary>
        /// The when_requesting_using_default_temp_endpoint.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_requesting_using_default_temp_endpoint : RabbitMqFixture
        {
            /// <summary>
            /// The should_return_response.
            /// </summary>
            [Test]
            public void should_return_response()
            {
                int result = 0;

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request")
                        .WithDefaultCallbackEndpoint());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num * 2))));

                Task<int> response = producer
                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13))
                    .ContinueWith(m => result = m.Result.Num);

                response.Wait(3.Seconds()).Should().BeTrue();
                result.Should().Be(26);
            }
        }

        [TestFixture]
        [Category("Integration")]
        public class when_response_is_null : RabbitMqFixture
        {
            [Test]
            public void should_receive_successfully()
            {
                DummyResponse result = new DummyResponse(666);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("dummy.request").WithDefaultCallbackEndpoint());
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request").ReactWith((m, ctx) => ctx.Reply<DummyResponse>(null)));

                Task<DummyResponse> response = producer
                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13))
                    .ContinueWith(m => result = m.Result);

                response.Wait(3.Seconds()).Should().BeTrue();
                result.Should().BeNull();
            }
        }

        /// <summary>
        /// При отравке нескольких запросов
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_request_multiple : RabbitMqFixture
        {
            /// <summary>
            /// Ответы должны приходить.
            /// </summary>
            [Test]
            public void should_receive_successfully()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("message1")
                                .WithDefaultCallbackEndpoint();
                            cfg.Route("message2")
                                .WithDefaultCallbackEndpoint();
                        });

                int counter1 = 0;
                int counter2 = 0;

                var rand1 = new Random(Environment.TickCount);
                var rand2 = new Random(Environment.TickCount / 2);

                const int Iterations = 1000;
                var countdown = new CountdownEvent(Iterations * 2);

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<Message1>("message1")
                               .ReactWith(
                                   (m, ctx) =>
                                   {
                                       Thread.Sleep(rand1.Next(5));
                                       ctx.Reply(new Message1 { Ticks = m.Ticks + 1 });
                                   }));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<Message2>("message2")
                                .ReactWith(
                                    (m, ctx) =>
                                    {
                                        Thread.Sleep(rand2.Next(5));
                                        ctx.Reply(new Message2 { Count = m.Count + 1 });
                                    }));

                producer.WhenReady.WaitOne();

                Enumerable.Range(0, Iterations).ForEach(
                    i =>
                    {
                        producer.RequestAsync<Message1, Message1>("message1", new Message1 { Ticks = counter1 })
                            .ContinueWith(
                                t =>
                                {
                                    Debug.Assert(t.Result.Ticks != null, "t.Result.Ticks != null");
                                    Interlocked.Exchange(ref counter1, t.Result.Ticks.Value);
                                    countdown.Signal();
                                });
                        producer.RequestAsync<Message2, Message2>("message2", new Message2 { Count = counter2 })
                            .ContinueWith(
                                t =>
                                {
                                    Debug.Assert(t.Result.Count != null, "t.Result.Count != null");
                                    Interlocked.Exchange(ref counter2, t.Result.Count.Value);
                                    countdown.Signal();
                                });
                    });

                countdown.Wait(30.Seconds()).Should().BeTrue();
            }
        }

        /// <summary>
        /// При потреблении сообщения.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class WhenConsumeMessage : RabbitMqFixture
        {
            /// <summary>
            /// Можно ответить на запрос.
            /// </summary>
            [Test]
            public void CanReplyIfRequest()
            {
                var autoReset = new AutoResetEvent(false);
                string messageLabel = "command.handle.this";
                var producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route(messageLabel).WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On(messageLabel).ReactWith<object>(
                        (m, ctx) =>
                            {
                                if (ctx.CanReply)
                                {
                                    autoReset.Set();
                                }

                                ctx.Reply(new object());
                            }));

                producer.Request<object, object>(messageLabel, new object(), new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }, o => { });
                Assert.IsTrue(autoReset.WaitOne(TimeSpan.FromSeconds(1)), "Должен быть получен запрос.");
            }

            /// <summary>
            /// Нельзя ответить, если не запрос.
            /// </summary>
            [Test]
            public void CanNotReplyIfEmit()
            {
                var autoReset = new AutoResetEvent(false);
                string messageLabel = "command.handle.this";
                var producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route(messageLabel).WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On(messageLabel).ReactWith<object>(
                        (m, ctx) =>
                        {
                            if (!ctx.CanReply)
                            {
                                autoReset.Set();
                            }
                        }));

                producer.Emit(messageLabel, new object());
                Assert.IsTrue(autoReset.WaitOne(TimeSpan.FromSeconds(1)), "Должен быть получен запрос.");
            }

            /// <summary>
            /// Можно обработать сообщение, метка которого отличается от метки получателя.
            /// </summary>
            [Test]
            public void CanConsumeWithWrongLabel()
            {
                var autoReset = new AutoResetEvent(false);
                string messageLabel = "command.handle.this";
                var producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route(messageLabel));

                var consumer = this.StartBus(
                    "consumer",
                    cfg =>
                    cfg
                        .On<object>(messageLabel + ".new")
                        .ReactWith(
                            m =>
                                {
                                    autoReset.Set();
                                })
                        .WithEndpoint(
                            builder =>
                                {
                                    Exchange e = builder.Topology.Declare(Exchange.Named(messageLabel).Fanout.Durable);
                                    Queue q = builder.Topology.Declare(Queue.Named(messageLabel + ".new"));

                                    builder.Topology.Bind(e, q);

                                    return new SubscriptionEndpoint(q, new StaticRouteResolver(e));
                                }));

                producer.Emit(messageLabel, new object());
                Assert.IsTrue(autoReset.WaitOne(TimeSpan.FromSeconds(1)), "Сообщение должно быть получено.");
                consumer.WhenReady.WaitOne();
            }
        }

        internal class Message1
        {
            public int? Ticks { get; set; }
        }

        internal class Message2
        {
            public int? Count { get; set; }
        }
    }

    // ReSharper restore InconsistentNaming
}
