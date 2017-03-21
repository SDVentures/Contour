using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contour.Testing.Plumbing;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ;
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

            [Test]
            public void bus_receiver_should_consume_at_each_url_of_connection_string()
            {
                var result = 0;

                var vhost1 = "test" + Guid.NewGuid().ToString("n") + "_1";
                var vhost2 = "test" + Guid.NewGuid().ToString("n") + "_2";

                this.Broker.CreateHost(vhost1);
                this.Broker.CreateUser(vhost1, this.TestUserName, this.TestUserPassword);
                this.Broker.SetPermissions(vhost1, this.TestUserName);

                this.Broker.CreateHost(vhost2);
                this.Broker.CreateUser(vhost2, this.TestUserName, this.TestUserPassword);
                this.Broker.SetPermissions(vhost2, this.TestUserName);

                var url1 = $"{this.Url}{vhost1}";
                this.ConnectionString = url1;
                var producer1 = this.StartBus(
                    "producer-1",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(url1)
                            .WithDefaultCallbackEndpoint();
                    });

                var url2 = $"{this.Url}{vhost2}";
                this.ConnectionString = url2;
                var producer2 = this.StartBus(
                    "producer-2",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(url2)
                            .WithDefaultCallbackEndpoint();
                    });

                var tcs = new TaskCompletionSource<bool>();
                var consumerConnectionString = $"{url1},{url2}";
                this.ConnectionString = consumerConnectionString;

                this.StartBus(
                    "consumer",
                    cfg => cfg
                        .On<DummyRequest>("dummy.request")
                        .WithConnectionString(consumerConnectionString)
                        .ReactWith((m, ctx) =>
                        {
                            result = m.Num * 3;
                            tcs.SetResult(true);
                        }));

                producer1.Emit("dummy.request", new DummyRequest(10));
                
                tcs.Task.Wait(10.Seconds());
                result.Should().Be(30);

                tcs = new TaskCompletionSource<bool>();
                producer2.Emit("dummy.request", new DummyRequest(20));

                tcs.Task.Wait(10.Seconds());
                result.Should().Be(60);
            }

            [Test]
            public void bus_receiver_should_respond_at_each_url_of_connection_string()
            {
                var result = 0;

                var vhost1 = "test" + Guid.NewGuid().ToString("n") + "_1";
                var vhost2 = "test" + Guid.NewGuid().ToString("n") + "_2";

                this.Broker.CreateHost(vhost1);
                this.Broker.CreateUser(vhost1, this.TestUserName, this.TestUserPassword);
                this.Broker.SetPermissions(vhost1, this.TestUserName);

                this.Broker.CreateHost(vhost2);
                this.Broker.CreateUser(vhost2, this.TestUserName, this.TestUserPassword);
                this.Broker.SetPermissions(vhost2, this.TestUserName);

                var url1 = $"{this.Url}{vhost1}";
                this.ConnectionString = url1;
                var producer1 = this.StartBus(
                    "producer-1",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(url1)
                            .WithDefaultCallbackEndpoint();
                    });

                var url2 = $"{this.Url}{vhost2}";
                this.ConnectionString = url2;
                var producer2 = this.StartBus(
                    "producer-2",
                    cfg =>
                    {
                        cfg
                            .Route("dummy.request")
                            .WithConnectionString(url2)
                            .WithDefaultCallbackEndpoint();
                    });

                var consumerConnectionString = $"{url1},{url2}";
                this.ConnectionString = consumerConnectionString;
                this.StartBus(
                    "consumer",
                    cfg => cfg
                        .On<DummyRequest>("dummy.request")
                        .WithConnectionString(consumerConnectionString)
                        .ReactWith((m, ctx) => ctx.Reply(new DummyResponse(m.Num * 3))));

                var response1 = producer1
                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(10))
                    .ContinueWith(m => result = m.Result.Num);

                response1.Wait(10.Seconds());
                result.Should().Be(30);

                var response2 = producer2
                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(20))
                    .ContinueWith(m => result = m.Result.Num);

                response2.Wait(10.Seconds());
                result.Should().Be(60);
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

            [Test]
            public void bus_sender_should_use_next_producer_chosen_by_default_producer_selector_on_each_message()
            {
                var result = 0;
                var count = 4;
                var urls = new List<string>();
                var tcsList = new Task<bool>[count];

                for (var i = 0; i < count; i++)
                {
                    var vhost = "test" + Guid.NewGuid().ToString("n") + $"_{i}";

                    this.Broker.CreateHost(vhost);
                    this.Broker.CreateUser(vhost, this.TestUserName, this.TestUserPassword);
                    this.Broker.SetPermissions(vhost, this.TestUserName);

                    var url = $"{this.Url}{vhost}";
                    urls.Add(url);
                    tcsList[i] = new Task<bool>(() => true);
                }

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

                for (var i = 0; i < urls.Count; i++)
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
                            .ReactWith((m, ctx) => tcsList[index].Start());
                    });
                }

                for (var i = 0; i < urls.Count; i++)
                {
                    producer.Emit("dummy.request", new DummyRequest(i));
                }
                
                Task.WaitAll(tcsList, 1.Minutes()).Should().BeTrue();
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

            [Test]
            public void bus_sender_should_use_next_producer_chosen_by_default_producer_selector_on_each_message()
            {
                var result = 0;
                var count = 4;
                var urls = new List<string>();
                var tcsList = new Task<bool>[count];

                for (var i = 0; i < count; i++)
                {
                    var vhost = "test" + Guid.NewGuid().ToString("n") + $"_{i}";

                    this.Broker.CreateHost(vhost);
                    this.Broker.CreateUser(vhost, this.TestUserName, this.TestUserPassword);
                    this.Broker.SetPermissions(vhost, this.TestUserName);

                    var url = $"{this.Url}{vhost}";
                    urls.Add(url);
                    tcsList[i] = new Task<bool>(() => true);
                }

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

                for (var i = 0; i < urls.Count; i++)
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
                                tcsList[index].Start();
                            });
                    });
                }

                for (var i = 0; i < urls.Count; i++)
                {
                    var task = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(i));
                    task.Wait(1.Minutes()).Should().BeTrue();
                    task.Result.Num.Should().Be(i);
                }

                Task.WaitAll(tcsList, 1.Minutes()).Should().BeTrue();
            }
        }
    }
}
