using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

using FluentAssertions;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Sending;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Topology;

using NUnit.Framework;
using RabbitMQ.Client;

namespace Contour.RabbitMq.Tests
{
    using Configuration;
    using FluentAssertions.Extensions;

    /// <summary>
    /// The basic publishing specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class BasicPublishingSpecs
    {
        /// <summary>
        /// The when_consuming_messages_of_different_types_on_same_queue.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        [Ignore("This requirement is removed until the following is implemented: receiver configuration and topology should be evaluated before receiver start; this will let the bus check what labels should be listened on by each listener")]
        public class when_consuming_messages_of_different_labels_on_same_queue : RabbitMqFixture
        {
            /// <summary>
            /// The should_consume_with_valid_consumer.
            /// </summary>
            [Test]
            public void should_consume_with_valid_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            Func<IRouteResolverBuilder, IRouteResolver> routeResolverBuilder = b => b.Topology.Declare(Exchange.Named("multi.exchange"));

                            cfg.Route("boo.request")
                                .ConfiguredWith(routeResolverBuilder);
                            cfg.Route("foo.request")
                                .ConfiguredWith(routeResolverBuilder);
                        });

                var consumer = this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> subscriptionBuilder = b =>
                                {
                                    Exchange e = b.Topology.Declare(Exchange.Named("multi.exchange"));
                                    Queue q = b.Topology.Declare(Queue.Named("multi.queue"));
                                    b.Topology.Bind(e, q);

                                    return new SubscriptionEndpoint(q, e);
                                };

                            cfg.On<BooMessage>("boo.request")
                                .ReactWith(
                                    (m, ctx) =>
                                        {
                                            result = m.Num;
                                            waitHandle.Set();
                                        })
                                .WithEndpoint(subscriptionBuilder);
                            cfg.On<FooMessage>("foo.request")
                                .ReactWith(
                                    (m, ctx) =>
                                        {
                                            result = m.Num * 2;
                                            waitHandle.Set();
                                        })
                                .WithEndpoint(subscriptionBuilder);
                        });

                consumer.WhenReady.WaitOne(10.Seconds()).Should().BeTrue();

                producer.Emit("boo.request", new BooMessage(13));
                
                waitHandle.WaitOne(1.Minutes()).Should().BeTrue();
                result.Should().Be(13);

                producer.Emit("foo.request", new FooMessage(13));

                waitHandle.WaitOne(1.Minutes()).Should().BeTrue();
                result.Should().Be(26);
            }
        }

        /// <summary>
        /// The when_publishing_anonymous_class_as_message.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_anonymous_class_as_message : RabbitMqFixture
        {
            /// <summary>
            /// The should_catch_message_on_subscribed_consumer.
            /// </summary>
            [Test]
            public void should_catch_message_on_subscribed_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo")
                            .ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = m.Num;
                                           waitHandle.Set();
                                       }));

                producer.Emit("boo", new { Num = 13 });

                waitHandle.WaitOne(5.Seconds()).Should().BeTrue();

                result.Should().Be(13);
            }
        }

        /// <summary>
        /// The when_publishing_simple_message_with_one_to_one_subscription.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_simple_message_with_one_to_one_subscription : RabbitMqFixture
        {
            /// <summary>
            /// The should_catch_message_on_subscribed_consumer.
            /// </summary>
            [Test]
            public void should_catch_message_on_subscribed_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = m.Num;
                                           waitHandle.Set();
                                       }));

                producer.Emit("boo", new BooMessage(13));

                waitHandle
                    .WaitOne(5.Seconds())
                    .Should()
                    .BeTrue();

                result.Should().Be(13);
            }
        }

        /// <summary>
        /// The when_publishing_using_any_label.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_using_any_label : RabbitMqFixture
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route(MessageLabel.Any));

                producer.Invoking(p => p.Emit(MessageLabel.Any, new BooMessage(13)))
                    .Should().Throw<InvalidOperationException>();
            }
        }

        /// <summary>
        /// При регистрации отправителя с возможностью послать сообщение с любой меткой.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_routing_using_any_label : RabbitMqFixture
        {
            /// <summary>
            /// Не должно генерироваться исключение при отправке сообщения.
            /// </summary>
            [Test]
            public void should_not_throw_when_emit()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route(MessageLabel.Any));

                producer.Invoking(p => p.Emit("dummy.request", new BooMessage(13)))
                    .Should().NotThrow();
            }

            /// <summary>
            /// Сообщение должно быть доставлено до адресата.
            /// </summary>
            [Test]
            public void should_consuming_message()
            {
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg
                        .Route(MessageLabel.Any)
                        .ConfiguredWith(builder =>
                            new LambdaRouteResolver(
                               (endpoint, label) =>
                                   {
                                       builder.Topology.Declare(Exchange.Named(label.Name).Durable.Fanout);
                                       return new RabbitRoute(label.Name);
                                   })));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").ReactWith(
                                   (m, ctx) => waitHandle.Set()));

                producer.Emit("boo", new BooMessage(13));

                waitHandle.WaitOne(5.Seconds()).Should().BeTrue();
            }

            /// <summary>
            /// Сообщение при динамическом и статическом роутинге должно быть доставлено до адресата.
            /// </summary>
            [Test]
            public void should_consuming_dynamic_and_static_message()
            {
                var waitDynamicHandle = new AutoResetEvent(false);
                var waitStaticHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route(MessageLabel.Any)
                                .ConfiguredWith(
                                    builder =>
                                    new LambdaRouteResolver(
                                        (endpoint, label) =>
                                            {
                                                builder.Topology.Declare(
                                                    Exchange.Named(label.Name)
                                                        .Durable.Fanout);
                                                return new RabbitRoute(label.Name);
                                            }));
                            cfg.Route("boo2");
                        });

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").ReactWith(
                                   (m, ctx) => waitDynamicHandle.Set()));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo2").ReactWith(
                                   (m, ctx) => waitStaticHandle.Set()));

                producer.Emit("boo", new BooMessage(13));
                producer.Emit("boo2", new BooMessage(13));

                WaitHandle.WaitAll(new WaitHandle[] { waitDynamicHandle, waitStaticHandle }, 5.Seconds()).Should().BeTrue();
            }

            /// <summary>
            /// Может быть получен ответ на такой запрос.
            /// </summary>
            [Test]
            public void should_receive_response()
            {
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg
                        .Route(MessageLabel.Any)
                        .ConfiguredWith(builder =>
                            new LambdaRouteResolver(
                               (endpoint, label) =>
                               {
                                   builder.Topology.Declare(Exchange.Named(label.Name).Durable.Fanout);
                                   return new RabbitRoute(label.Name);
                               }))
                        .WithDefaultCallbackEndpoint());

                var consumer = this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").ReactWith((m, ctx) => ctx.Reply(new { })));

                producer.WhenReady.WaitOne();
                consumer.WhenReady.WaitOne();

                producer.Request("boo", new { }, new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }, (object message) => waitHandle.Set());

                waitHandle.WaitOne(5.Seconds()).Should().BeTrue();
            }

            /// <summary>
            /// Может быть получен ответ на любые типы запросов..
            /// </summary>
            [Test]
            public void should_receive_response_from_both()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg
                        .Route(MessageLabel.Any)
                        .ConfiguredWith(builder =>
                            new LambdaRouteResolver(
                               (endpoint, label) =>
                               {
                                   builder.Topology.Declare(Exchange.Named(label.Name).Durable.Fanout);
                                   return new RabbitRoute(label.Name);
                               }))
                        .WithDefaultCallbackEndpoint());

                int counter1 = 0;
                int counter2 = 0;

                var rand1 = new Random(Environment.TickCount);
                var rand2 = new Random(Environment.TickCount / 2);

                int iterations = 50;
                var countdown = new CountdownEvent(iterations * 2);

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
                    cfg => cfg.On<Message2>("message2").ReactWith(
                        (m, ctx) =>
                            {
                                Thread.Sleep(rand2.Next(5));
                                ctx.Reply(new Message2 { Count = m.Count + 1 });
                            }));

                producer.WhenReady.WaitOne();

                Enumerable.Range(0, iterations).ForEach(
                    i =>
                        {
                            producer.RequestAsync<Message1, Message1>("message1", new Message1 { Ticks = counter1 })
                                .ContinueWith(
                                    t =>
                                        {
                                            if (t.Result.Ticks != null)
                                            {
                                                Interlocked.Exchange(ref counter1, t.Result.Ticks.Value);
                                            }

                                            countdown.Signal();
                                        });
                            producer.RequestAsync<Message2, Message2>("message2", new Message2 { Count = counter2 })
                                .ContinueWith(
                                    t =>
                                        {
                                            if (t.Result.Count != null)
                                            {
                                                Interlocked.Exchange(ref counter2, t.Result.Count.Value);
                                            }

                                            countdown.Signal();
                                        });
                        });

                countdown.Wait(30.Seconds()).Should().BeTrue();
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
    }
    // ReSharper restore InconsistentNaming
}
