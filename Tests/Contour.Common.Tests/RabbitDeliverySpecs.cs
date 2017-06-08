using System;
using System.Collections.Generic;

using Contour.Serialization;
using Contour.Transport.RabbitMq;
using Contour.Transport.RabbitMq.Internal;

using Moq;

using NUnit.Framework;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9_1;

namespace Contour.Common.Tests
{
    [TestFixture]
    public class RabbitDeliverySpecs
    {
        [Test]
        public void when_reply_with_content_type_should_insert_content_type_into_message_headers()
        {
            Mock<IMessageLabelHandler> messageLabelHandlerMock = new Mock<IMessageLabelHandler>();

            Mock<IBusContext> busContextMock = new Mock<IBusContext>();
            busContextMock.SetupGet(ctx => ctx.MessageLabelHandler)
                .Returns(messageLabelHandlerMock.Object);

            Mock<IPayloadConverter> payloadConverterMock = new Mock<IPayloadConverter>();

            var args = new BasicDeliverEventArgs();
            args.BasicProperties = new BasicProperties();
            args.BasicProperties.ReplyTo = "some.address";
            args.BasicProperties.ReplyToAddress = new PublicationAddress("test", "test", "test");
            args.BasicProperties.CorrelationId = Guid.NewGuid().ToString("N");
            args.BasicProperties.ContentType = "application/json";

            Mock<IRabbitChannel> rabbitChannelMock = new Mock<IRabbitChannel>();
            IMessage resultMessage = null;
            rabbitChannelMock.Setup(
                rc => rc.Publish(
                    It.IsAny<IRoute>(),
                    It.IsAny<IMessage>(),
                    It.IsAny<IPayloadConverter>()))
                .Callback<IRoute, IMessage, IPayloadConverter>((r, m, c) => { resultMessage = m; });

            Mock<IMessage> messageMock = new Mock<IMessage>();
            IDictionary<string, object> messageHeaders = new Dictionary<string, object>();
            messageMock.Setup(m => m.Headers).Returns(messageHeaders);

            var sut = new RabbitDelivery(
                busContextMock.Object, 
                rabbitChannelMock.Object, 
                args, 
                true, 
                payloadConverterMock.Object);

            sut.ReplyWith(messageMock.Object);

            Assert.IsTrue(messageHeaders.ContainsKey(Headers.ContentType));
            Assert.IsTrue(resultMessage.Headers == messageHeaders);
        }
    }
}