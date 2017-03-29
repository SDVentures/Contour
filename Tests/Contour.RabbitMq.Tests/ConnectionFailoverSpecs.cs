using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contour.Testing.Plumbing;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;
using FluentAssertions;
using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),
        SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."),
        Category("Integration")]
    public class ConnectionFailoverSpecs : RabbitMqFixture
    {
        [SetUp]
        public override void SetUp()
        {
            this.PreSetUp();

            this.Endpoints = new List<IBus>();
            this.Broker = new Broker(this.ManagementConnection, this.AdminUserName, this.AdminUserPassword);
        }

        [TearDown]
        public override void TearDown()
        {
            this.DeleteHosts();
        }

        [Test]
        public void should_recover_listener_after_failure_on_connection_restore()
        {
            const int Count = 100;
            var label = "recover.request";
            var tcsList = new List<TaskCompletionSource<bool>>(Count);
            
            this.ConnectionString = this.InitializeHosts(1).First();

            var producer = this.StartBus("producer", cfg => cfg.Route(label));

            for (int i = 0; i < Count; i++)
            {
                tcsList.Add(new TaskCompletionSource<bool>());
                producer.Emit(label, new DummyRequest(i));
                Trace.WriteLine($"Message {i} published");
            }

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    this.Broker.DropConnections();
                    Trace.WriteLine("Broker connections dropped");
                    Thread.Sleep(100.Milliseconds());
                }
            });

            // Why RequiresAccept should be true: since the consumer may enqueue some messages before the connection is dropped these messages need to be rejected by the channel and re-queued by the broker; to make this happen the messages need to be acknowledged explicitly; otherwise these messages are going to be lost
            this.StartBus(
                "consumer",
                cfg => cfg
                    .On<DummyRequest>(label)
                    .RequiresAccept()
                    .ReactWith((m, ctx) =>
                    {
                        Trace.WriteLine($"Message received {m.Num}");
                        try
                        {
                            tcsList[m.Num].SetResult(true);
                            ctx.Accept();
                        }
                        catch{}
                    }));
            
            Assert.True(Task.WaitAll(tcsList.Select(tcs => tcs.Task).ToArray(), 5.Minutes()));
        }

        [Test]
        public void should_recover_producer_on_message_publish_failure()
        {
            Assert.Ignore("Not implemented");
        }

        [Test]
        public void should_not_recover_producer_on_message_publish_confirmation_failure()
        {
            const int Count = 500;
            const string Label = "dummy.request";
            int published = 0, unconfirmed = 0, failed = 0;
            
            this.ConnectionString = this.InitializeHosts(1).First();

            var producer = this.StartBus(
                "producer",
                cfg =>
                {
                    cfg
                        .Route(Label)
                        .WithConfirmation();
                });
            producer.WhenReady.WaitOne();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    this.Broker.DropConnections();
                    Trace.WriteLine("Broker connections dropped");
                    Thread.Sleep(100.Milliseconds());
                }
            });

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
        }
    }
}
