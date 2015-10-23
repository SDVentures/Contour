using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The performance request response specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class PerformanceRequestResponseSpecs
    {
        /// <summary>
        /// The when_processing_many_requests_in_parallel.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_processing_many_requests_in_parallel : RabbitMqFixture
        {
            /// <summary>
            /// The should_process_fast_enough.
            /// </summary>
            [Test]
            [Ignore("Too long")]
            public void should_process_fast_enough()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(4);
                            cfg.Route("dummy.request")
                                .WithCallbackEndpoint(b => b.UseDefaultTempReplyEndpoint());
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(4);
                            cfg.On<DummyRequest>("dummy.request")
                                .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num)));
                        });

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                int total = 0;
                const int TotalOps = 30000;

                Task[] tasks = Enumerable.Range(0, TotalOps)
                    .Select(
                        n => producer
                            .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(n))
                            .ContinueWith(t => Interlocked.Increment(ref total)))
                    .Cast<Task>()
                    .ToArray();

                Task.WaitAll(tasks);
                stopwatch.Stop();

                total.Should().Be(TotalOps);

                double opsPerSecond = TotalOps / stopwatch.Elapsed.TotalSeconds;

                Console.WriteLine("{0} ops/s", opsPerSecond);

                opsPerSecond.Should().BeGreaterThan(6000);
            }
        }

        /// <summary>
        /// The when_processing_many_requests_serialized.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_processing_many_requests_serialized : RabbitMqFixture
        {
            /// <summary>
            /// The should_process_fast_enough.
            /// </summary>
            [Test]
            [Ignore("Too long")]
            public void should_process_fast_enough()
            {
                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(1);
                            cfg.Route("dummy.request")
                                .WithCallbackEndpoint(b => b.UseDefaultTempReplyEndpoint());
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.UseParallelismLevel(1);
                            cfg.On<DummyRequest>("dummy.request")
                                .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num)));
                        });

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                int total = 0;
                const int TotalOps = 30000;

                Task[] tasks = Enumerable.Range(0, TotalOps)
                    .Select(
                        n => producer
                            .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(n))
                            .ContinueWith(t => Interlocked.Increment(ref total)))
                    .Cast<Task>()
                    .ToArray();

                Task.WaitAll(tasks);
                stopwatch.Stop();

                total.Should().Be(TotalOps);

                double opsPerSecond = TotalOps / stopwatch.Elapsed.TotalSeconds;

                Console.WriteLine("{0} ops/s", opsPerSecond);

                opsPerSecond.Should().BeGreaterThan(6000);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
