using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contour.Configuration;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMq;

using FluentAssertions;
using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),
     SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."),
     Category("Manual")]
    [Ignore("These tests can be run manually to check failover behavior but are not guaranteed to be successful while testing automatically")]
    public class ConnectionFailoverSpecs : RabbitMqFixture
    {
        [TearDown]
        public override void TearDown()
        {
            this.DeleteHosts();
            base.TearDown();
        }

        [Test]
        public void should_recover_listener_after_failure_on_connection_restore()
        {
            const int Count = 100;
            var label = "listener.recover.request";
            var tcsList = new List<TaskCompletionSource<bool>>(Count);

            // Why RequiresAccept should be true: since the consumer may enqueue some messages before the connection is dropped these messages need to be rejected by the channel and re-queued by the broker; to make this happen the messages need to be acknowledged explicitly; otherwise these messages are going to be lost
            var consumer = this.StartBus(
                "consumer",
                cfg => cfg
                    .On<DummyRequest>(label)
                    .WithQoS(1, 0)
                    .RequiresAccept()
                    .ReactWith((m, ctx) =>
                    {
                        try
                        {
                            Trace.WriteLine($"Message received {m.Num}");
                            tcsList[m.Num].SetResult(true);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            ctx.Accept();
                        }

                    }));
            consumer.WhenReady.WaitOne();
            consumer.Stop();

            var producer = this.StartBus("producer", cfg =>
            {
                cfg.Route(label);

            });
            producer.WhenReady.WaitOne();

            for (int i = 0; i < Count; i++)
            {
                tcsList.Add(new TaskCompletionSource<bool>());
                producer.Emit(label, new DummyRequest(i)).Wait();
                Trace.WriteLine($"Message {i} published");
            }

            consumer.Start();

            var cts = new CancellationTokenSource();
            var dropTask = this.CreateConnectionDropTask(cts.Token);
            dropTask.Start();

            Assert.True(Task.WaitAll(tcsList.Select(tcs => tcs.Task).ToArray(), 5.Minutes()));
            cts.Cancel();
        }

        [Test]
        public void should_not_recover_producer_on_message_publish_confirmation_failure()
        {
            const int Count = 300;
            const string Label = "dummy.request";
            int published = 0, unconfirmed = 0, failed = 0;

            var producer = this.StartBus(
                "producer",
                cfg =>
                {
                    cfg
                        .Route(Label)
                        .WithConfirmation();
                });
            producer.WhenReady.WaitOne();

            var cts = new CancellationTokenSource();
            var dropTask = this.CreateConnectionDropTask(cts.Token);
            dropTask.Start();

            for (int i = 0; i < Count; i++)
            {
                try
                {
                    producer.Emit(Label, new DummyRequest(i)).Wait();
                    Trace.WriteLine($"Message {i} published");
                    published++;
                }
                catch (UnconfirmedMessageException umex)
                {
                    Trace.WriteLine($"Unconfirmed message [{umex.SequenceNumber}], resending");
                    unconfirmed++;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Message emit failed due to [{ex.Message}]");
                    failed++;
                }
            }

            Trace.WriteLine($"Published={published}, unconfirmed={unconfirmed}, failed={failed}");
            Assert.True(published + unconfirmed + failed == Count);
            cts.Cancel();
        }

        [Test]
        public void should_stop_the_bus_while_producer_is_failing_over()
        {
            const string Label = "dummy.request";

            var producer = this.StartBus(
                "producer",
                cfg =>
                {
                    cfg
                        .Route(Label)
                        .WithConfirmation();
                });
            producer.WhenReady.WaitOne();

            var cts = new CancellationTokenSource();
            var dropTask = this.CreateConnectionDropTask(cts.Token);
            dropTask.Start();
            dropTask.Wait(5.Seconds());

            producer.Stop();
            cts.Cancel();
        }

        [Test]
        public void should_stop_the_bus_while_listener_is_failing_over()
        {
            const string Label = "dummy.request";

            var consumer = this.StartBus(
                "consumer",
                cfg => cfg
                    .On<DummyRequest>(Label)
                    .ReactWith((m, ctx) =>
                    {
                        Trace.WriteLine($"Message received {m.Num}");
                    }));

            var cts = new CancellationTokenSource();
            var dropTask = this.CreateConnectionDropTask(cts.Token);
            dropTask.Start();
            dropTask.Wait(5.Seconds());

            consumer.Stop();
            cts.Cancel();
        }


        private Task CreateConnectionDropTask(CancellationToken token)
        {
            return new Task(
                () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        this.Broker.DropConnections();
                        Trace.WriteLine("Broker connections dropped");
                        Thread.Sleep(100.Milliseconds());
                    }
                },
                token);
        }
    }
}
