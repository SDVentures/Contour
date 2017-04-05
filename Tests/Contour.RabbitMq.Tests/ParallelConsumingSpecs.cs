using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using FluentAssertions;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The parallel consuming specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class ParallelConsumingSpecs
    {
        /// <summary>
        /// The when_consuming_multiple_messages_on_different_queues.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_multiple_messages_on_different_queues : RabbitMqFixture
        {
            /// <summary>
            /// The should_consume_in_parallel.
            /// </summary>
            [Test]
            public void should_consume_in_parallel()
            {
                int result = 0;
                const int target = 5;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("dummy.request.1");
                            cfg.Route("dummy.request.2");
                            cfg.Route("dummy.request.3");
                            cfg.Route("dummy.request.4");
                            cfg.Route("dummy.request.5");
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            Action<DummyRequest, IConsumingContext<DummyRequest>> handler = (request, context) =>
                                {
                                    Interlocked.Add(ref result, request.Num);
                                    if (result == target)
                                    {
                                        waitHandle.Set();
                                    }

                                    Thread.Sleep(1.Seconds());
                                };

                            cfg.On<DummyRequest>("dummy.request.1").
                                ReactWith(handler);
                            cfg.On<DummyRequest>("dummy.request.2").
                                ReactWith(handler);
                            cfg.On<DummyRequest>("dummy.request.3").
                                ReactWith(handler);
                            cfg.On<DummyRequest>("dummy.request.4").
                                ReactWith(handler);
                            cfg.On<DummyRequest>("dummy.request.5").
                                ReactWith(handler);
                        });

                Enumerable.Range(0, target).
                    ForEach(n => producer.Emit("dummy.request." + (n + 1), new DummyRequest(1)));

                waitHandle.WaitOne(1.Seconds()).
                    Should().
                    BeTrue();
            }
        }

        /// <summary>
        /// The when_consuming_multiple_messages_on_the_same_queue_using_one_worker.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_multiple_messages_on_the_same_queue_using_one_worker : RabbitMqFixture
        {
            /// <summary>
            /// The message count.
            /// </summary>
            private const int MessageCount = 4;
            /// <summary>
            /// The wait handle.
            /// </summary>
            public CountdownEvent WaitHandle = new CountdownEvent(MessageCount);
            /// <summary>
            /// The should_consume_serialized.
            /// </summary>
            [Test]
            public void should_consume_serialized()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route("dummy.request"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(1);

                            cfg.On<DummyRequest>("dummy.request").
                                ReactWith(
                                    (m, ctx) =>
                                        {
                                            this.WaitHandle.Signal();
                                            Thread.Sleep(1.Seconds());
                                        });
                        });

                Enumerable.Range(0, MessageCount).
                    ForEach(_ => producer.Emit("dummy.request", new DummyRequest(1)));

                this.WaitHandle.Wait(2.Seconds()).
                    Should().
                    BeFalse();
            }
        }

        /// <summary>
        /// The when_consuming_multiple_messages_on_the_same_queue_using_several_workers.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_multiple_messages_on_the_same_queue_using_several_workers : RabbitMqFixture
        {
            /// <summary>
            /// The message count.
            /// </summary>
            private const int MessageCount = 4;
            /// <summary>
            /// The wait handle.
            /// </summary>
            public CountdownEvent WaitHandle = new CountdownEvent(MessageCount);
            /// <summary>
            /// The should_consume_in_parallel.
            /// </summary>
            [Test]

            // [Ignore("Unstable")]
            public void should_consume_in_parallel()
            {
                IBus producer = this.StartBus("producer", cfg => cfg.Route("dummy.request"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(4);

                            cfg.On<DummyRequest>("dummy.request").
                                ReactWith(
                                    (m, ctx) =>
                                        {
                                            this.WaitHandle.Signal();
                                            Thread.Sleep(1.Seconds());
                                        });
                        });

                Enumerable.Range(0, MessageCount).
                    ForEach(_ => producer.Emit("dummy.request", new DummyRequest(1)));

                this.WaitHandle.Wait(2.Seconds()).
                    Should().
                    BeTrue();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
