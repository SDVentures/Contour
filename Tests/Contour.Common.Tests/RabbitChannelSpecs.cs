using System;
using System.Collections.Generic;

using Contour.Serialization;
using Contour.Transport.RabbitMq;
using Contour.Transport.RabbitMq.Internal;

using Moq;

using NUnit.Framework;

using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

// ReSharper disable InconsistentNaming

namespace Contour.Common.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class RabbitChannelSpecs
    {
        [Test]
        public void when_publish_message_should_choose_payload_converter()
        {
            Mock<IModel> modelMock = new Mock<IModel>();
            modelMock.Setup(m => m.CreateBasicProperties())
                .Returns(new BasicProperties());

            Mock<IMessageLabelHandler> messageLabelHandlerMock = new Mock<IMessageLabelHandler>();

            Mock<IBusContext> busContextMock = new Mock<IBusContext>();
            busContextMock.SetupGet(ctx => ctx.MessageLabelHandler)
                .Returns(messageLabelHandlerMock.Object);

            var contentType = "application/protobuf";
            var payloadConverterMock = this.CreateConverter(contentType);

            Mock<IPayloadConverterResolver> payloadConverterResolverMock = new Mock<IPayloadConverterResolver>();
            payloadConverterResolverMock.Setup(
                pcr => pcr.ResolveConverter(It.IsAny<string>()))
                .Returns(payloadConverterMock.Object);

            var sut = new RabbitChannel(Guid.NewGuid(), modelMock.Object, busContextMock.Object, payloadConverterResolverMock.Object);
            RabbitRoute route = new RabbitRoute("some.exchange");
            Mock<IMessage> messageMock = new Mock<IMessage>();
            messageMock.SetupGet(m => m.Headers)
                .Returns(new Dictionary<string, object>());

            sut.Publish(route, messageMock.Object);

            modelMock.Verify(
                m => m.BasicPublish(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<bool>(), 
                    It.Is<IBasicProperties>(
                        p => p.IsContentTypePresent() && p.ContentType == contentType),
                    It.IsAny<byte[]>()
                    ),
                Times.Once,
                "should choose the content type by message header.");

            payloadConverterMock.Verify(pc => pc.FromObject(It.IsAny<object>()), Times.Once);
        }

        [Test]
        public void when_reply_message_should_choose_payload_converter()
        {
            Mock<IModel> modelMock = new Mock<IModel>();
            IBasicProperties messageProperties = new BasicProperties();
            modelMock.Setup(m => m.CreateBasicProperties()).Returns(messageProperties);

            Mock<IMessageLabelHandler> messageLabelHandlerMock = new Mock<IMessageLabelHandler>();

            Mock<IBusContext> busContextMock = new Mock<IBusContext>();
            busContextMock.SetupGet(ctx => ctx.MessageLabelHandler)
                .Returns(messageLabelHandlerMock.Object);

            var contentType = "application/protobuf";
            var payloadConverterMock = this.CreateConverter(contentType);

            Mock<IPayloadConverterResolver> payloadConverterResolverMock = new Mock<IPayloadConverterResolver>();
            payloadConverterResolverMock.Setup(
                pcr => pcr.ResolveConverter(It.IsAny<string>()))
                .Returns(payloadConverterMock.Object);

            Mock<IMessage> messageMock = new Mock<IMessage>();
            IDictionary<string, object> messageHeaders = new Dictionary<string, object>();
            messageMock.SetupGet(m => m.Headers).Returns(messageHeaders);

            RabbitRoute route = new RabbitRoute("some.exchange");

            var sut = new RabbitChannel(Guid.NewGuid(), modelMock.Object, busContextMock.Object, payloadConverterResolverMock.Object);

            sut.Reply(messageMock.Object, route, Guid.NewGuid().ToString("N"));

            Assert.AreEqual(contentType, messageProperties.ContentType);
        }


        private Mock<IPayloadConverter> CreateConverter(string contentType)
        {
            var converterMock = new Mock<IPayloadConverter>();
            converterMock.SetupGet(c => c.ContentType).Returns(contentType);
            return converterMock;
        }
    }
}