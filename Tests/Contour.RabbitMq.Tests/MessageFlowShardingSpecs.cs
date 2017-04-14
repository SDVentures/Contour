using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contour.Configuration;
using Contour.Testing.Plumbing;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMq;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class MessageFlowShardingSpecs
    {
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_using_connection_string_with_multiple_urls : RabbitMqFixture
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
            public void bus_receiver_should_consume_at_each_url_of_connection_string()
            {
                var result = 0;
                const int Count = 4;
                const int Multiplier = 3;

                var urls = this.InitializeHosts(Count).ToArray();
                var producers = new List<IBus>();

                for (var i = 0; i < Count; i++)
                {
                    this.ConnectionString = urls[i];
                    var index = i;
                    var producer = this.StartBus(
                        $"producer-{i}",
                        cfg =>
                        {
                            cfg
                                .Route("dummy.request")
                                .WithConnectionString(urls[index])
                                .WithDefaultCallbackEndpoint();
                        });

                    producers.Add(producer);
                }

                TaskCompletionSource<bool> tcs = null;

                var consumerConnectionString = string.Join(",", urls);
                this.ConnectionString = consumerConnectionString;
                
                this.StartBus(
                    "consumer",
                    cfg => cfg
                        .On<DummyRequest>("dummy.request")
                        .WithConnectionString(consumerConnectionString)
                        .ReactWith((m, ctx) =>
                        {
                            result = m.Num * Multiplier;
                            tcs.SetResult(true);
                        }));

                for (var i = 0; i < Count; i++)
                {
                    tcs = new TaskCompletionSource<bool>();
                    producers[i].Emit("dummy.request", new DummyRequest(i));
                    tcs.Task.Wait(1.Minutes());

                    result.Should().Be(i * Multiplier);
                }
            }

            [Test]
            public void bus_receiver_should_respond_at_each_url_of_connection_string()
            {
                var result = 0;
                const int Count = 4;
                const int Multiplier = 3;

                var urls = this.InitializeHosts(Count).ToArray();
                var producers = new List<IBus>();

                for (var i = 0; i < Count; i++)
                {
                    this.ConnectionString = urls[i];
                    var index = i;
                    var producer = this.StartBus(
                        $"producer-{i}",
                        cfg =>
                        {
                            cfg
                                .Route("dummy.request")
                                .WithConnectionString(urls[index])
                                .WithDefaultCallbackEndpoint();
                        });

                    producers.Add(producer);
                }
                
                var consumerConnectionString = string.Join(",", urls);
                this.ConnectionString = consumerConnectionString;
                
                this.StartBus(
                    "consumer",
                    cfg => cfg
                        .On<DummyRequest>("dummy.request")
                        .WithConnectionString(consumerConnectionString).RequiresAccept()
                        .ReactWith((m, ctx) =>
                        {
                            ctx.Accept();
                            ctx.Reply(new DummyResponse(m.Num * Multiplier));
                        }));

                for (var i = 0; i < Count; i++)
                {
                    var response = producers[i].RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(i))
                    .ContinueWith(m => result = m.Result.Num);

                    response.Wait(1.Minutes());
                    result.Should().Be(i * Multiplier);
                }
            }
        }

        [TestFixture]
        [Category("Integration")]
        public class when_emitting_using_connection_string_with_multiple_ulrs : RabbitMqFixture
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
            public void bus_sender_should_use_next_producer_chosen_by_default_producer_selector_on_each_message()
            {
                const int Count = 4;
                var tasks = Enumerable.Range(0, Count).Select(i => new Task<bool>(() => true)).ToArray();
                var urls = this.InitializeHosts(Count).ToArray();
                
                var producerConnectionString = string.Join(",", urls);
                
                this.ConnectionString = producerConnectionString;
                var producer = this.StartBus(
                    "producer",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(producerConnectionString);
                    });

                for (var i = 0; i < urls.Length; i++)
                {
                    var conString = urls[i];
                    this.ConnectionString = conString;

                    var index = i;
                    this.StartBus(
                    $"consumer-{i}",
                    cfg =>
                    {
                        cfg
                            .On<DummyRequest>("dummy.request")
                            .WithConnectionString(conString)
                            .ReactWith((m, ctx) => tasks[index].Start());
                    });
                }

                for (var i = 0; i < urls.Length; i++)
                {
                    producer.Emit("dummy.request", new DummyRequest(i));
                }
                
                Task.WaitAll(tasks, 1.Minutes()).Should().BeTrue();
            }
        }

        [TestFixture]
        [Category("Integration")]
        public class when_requesting_using_connection_string_with_multiple_ulrs : RabbitMqFixture
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
            public void bus_sender_should_use_next_producer_chosen_by_default_producer_selector_on_each_message()
            {
                const int Count = 4;
                var tasks = Enumerable.Range(0, Count).Select(i => new Task<bool>(() => true)).ToArray();
                var urls = this.InitializeHosts(Count).ToArray();

                var producerConnectionString = string.Join(",", urls);

                this.ConnectionString = producerConnectionString;
                var producer = this.StartBus(
                    "producer",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(producerConnectionString)
                            .WithDefaultCallbackEndpoint();
                    });

                for (var i = 0; i < urls.Length; i++)
                {
                    var conString = urls[i];
                    this.ConnectionString = conString;

                    var index = i;
                    this.StartBus(
                    $"consumer-{i}",
                    cfg =>
                    {
                        cfg
                            .On<DummyRequest>("dummy.request")
                            .WithConnectionString(conString)
                            .ReactWith((m, ctx) =>
                            {
                                ctx.Reply(new DummyResponse(m.Num));
                                tasks[index].Start();
                            });
                    });
                }

                for (var i = 0; i < urls.Length; i++)
                {
                    var task = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(i));
                    task.Wait(1.Minutes()).Should().BeTrue();
                    task.Result.Num.Should().Be(i);
                }

                Task.WaitAll(tasks, 1.Minutes()).Should().BeTrue();
            }

            [Test]
            public void bus_sender_should_create_temporary_queue_for_each_url()
            {
                const int Count = 4;
                var urls = this.InitializeHosts(Count).ToArray();

                var producerConnectionString = string.Join(",", urls);

                this.ConnectionString = producerConnectionString;
                var producer = this.StartBus(
                    "producer",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(producerConnectionString)
                            .WithDefaultCallbackEndpoint();
                    });
                producer.WhenReady.WaitOne();

                foreach (var url in urls)
                {
                    var vhostName = this.GetVhost(url);
                    var queues = this.Broker.GetQueues(vhostName).Where(q => !q.Name.ToLower().Contains("fault")).ToList();
                    queues.Count.Should().Be(1, $"sender should create only one queue at {url}");
                }
            }
        }
    }
}
