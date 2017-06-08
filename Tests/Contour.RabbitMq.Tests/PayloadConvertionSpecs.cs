using System;
using System.Linq;
using System.Threading;

using Contour.Configuration;
using Contour.Sending;
using Contour.Serialization;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMq;

using FluentAssertions;

using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.RabbitMq.Tests
{
    [TestFixture]
    public class PayloadConvertionSpecs : RabbitMqFixture
    {
        [Test]
        public void when_publish_message_via_bus_with_two_converters_should_be_choosen_converter_by_message_options()
        {
            var contentType = "application/fake-protobuf";
            var bus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message");
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            bus.Start();
            this.Broker.CreateQueue(this.VhostName, "test");
            this.Broker.CreateBind(this.VhostName, "some.message", "test");
            bus.Emit("some.message", "test", new PublishingOptions { ContentType = contentType });
            var messages = this.Broker.GetMessages(this.VhostName, "test", 1, false);

            Assert.AreEqual(contentType, messages.First().Properties.ContentType);
        }

        [Test]
        public void when_forward_should_be_used_same_content_type()
        {
            var producerBus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message");
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-protobuf"));
                    });
            producerBus.Start();

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            var consumerBus = this.ConfigureBus(
                    "consumer",
                    cfg =>
                    {
                        cfg.On("some.message").ReactWith<string>((m, ctx) => 
                        {
                            ctx.Forward("another.message");
                            resetEvent.Set();
                        });
                        cfg.Route("another.message");
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-protobuf"));
                    });
            consumerBus.Start();

            this.Broker.CreateQueue(this.VhostName, "test");
            this.Broker.CreateBind(this.VhostName, "another.message", "test");
            producerBus.Emit("some.message", "test", new PublishingOptions { ContentType = "application/fake-protobuf" });
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(250)).Should().BeTrue();
            var messages = this.Broker.GetMessages(this.VhostName, "test", 1, false);

            Assert.AreEqual("application/fake-protobuf", messages.First().Properties.ContentType);
        }

        [Test]
        public void when_publish_without_converter_type_should_be_used_first()
        {
            var bus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message");
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-protobuf"));
                    });
            bus.Start();
            this.Broker.CreateQueue(this.VhostName, "test");
            this.Broker.CreateBind(this.VhostName, "some.message", "test");
            bus.Emit("some.message", "test");
            var messages = this.Broker.GetMessages(this.VhostName, "test", 1, false);

            Assert.AreEqual("application/json", messages.First().Properties.ContentType);
        }

        [Test]
        public void when_publish_with_invalid_contenttype_should_throw_exception()
        {
            var contentType = "application/fake-protobuf";
            var bus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message");
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            bus.Start();
            this.Broker.CreateQueue(this.VhostName, "test");
            this.Broker.CreateBind(this.VhostName, "some.message", "test");
            Assert.Throws<FailoverException>(() => bus.Emit("some.message", "test", new PublishingOptions { ContentType = contentType + "5" }));
        }

        [Test]
        public void when_reply_without_contenttype_should_use_input_content_type()
        {
            var contentType = "application/fake-protobuf";
            var producerBus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message").WithDefaultCallbackEndpoint();
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            producerBus.Start();

            var consumerBus = this.ConfigureBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.On("some.message")
                                .ReactWith<string>((m, ctx) => { ctx.Reply("test"); });
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            consumerBus.Start();

            string result = null;
            producerBus.Request<string, string>("some.message", "test", new RequestOptions { ContentType = contentType }, r => { result = r; });

            Assert.AreEqual(contentType, result);
        }

        [Test]
        public void when_request_with_invalid_content_type_should_throw_exception()
        {
            var contentType = "application/fake-protobuf";
            var producerBus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message").WithDefaultCallbackEndpoint();
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            producerBus.Start();

            Assert.Throws<AggregateException>(
                () => producerBus.Request<string, string>(
                    "some.message", 
                    "test", 
                    new RequestOptions
                        {
                            ContentType = contentType,
                            Timeout = TimeSpan.FromSeconds (1)
                        }, 
                    r => { }));
        }

        [Test]
        public void when_receive_with_invalid_content_type_should_throw_exception()
        {
            var contentType = "application/fake-protobuf";
            var producerBus = this.ConfigureBus(
                    "producer",
                    cfg =>
                    {
                        cfg.Route("some.message").WithDefaultCallbackEndpoint();
                        cfg.UsePayloadConverter(new FakePayloadConverter("application/fake-json"));
                        cfg.UsePayloadConverter(new FakePayloadConverter(contentType));
                    });
            producerBus.Start();

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            Exception resultException = null;
            var consumerBus = this.ConfigureBus(
                    "consumer",
                    cfg =>
                    {
                        cfg.On("some.message")
                            .ReactWith<string>((m, ctx) => { ctx.Reply("test"); });
                        cfg.OnFailed(context =>
                            {
                                resultException = context.Exception;
                                resetEvent.Set();
                            });
                    });
            consumerBus.Start();

            producerBus.Emit("some.message", "test", new PublishingOptions { ContentType = "application/fake-json" });
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(250)).Should().BeTrue();
            resultException.Should().NotBeNull();
            resultException.Should().BeOfType<ArgumentOutOfRangeException>();
        }


        class FakePayloadConverter : IPayloadConverter
        {
            public FakePayloadConverter(string contentType)
            {
                this.ContentType = contentType;
            }

            public string ContentType { get; }

            public byte[] FromObject(object payload)
            {
                return System.Text.Encoding.UTF8.GetBytes(this.ContentType);
            }

            public object ToObject(byte[] payload, Type targetType)
            {
                return System.Text.Encoding.UTF8.GetString(payload);
            }
        }
    }
}