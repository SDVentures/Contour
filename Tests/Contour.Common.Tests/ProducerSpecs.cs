using System.Collections.Generic;
using System.Threading;

using Contour.Sending;
using Contour.Serialization;
using Contour.Transport.RabbitMq.Internal;

using Moq;

using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Contour.Common.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class ProducerSpecs
    {
        [Test]
        public void when_publish_message_should_choose_payload_converter()
        {
            var contentType = "application/protobuf";
            var payloadConverterMock = this.CreateConverter(contentType);

            Mock<IPayloadConverterResolver> payloadConverterResolverMock = new Mock<IPayloadConverterResolver>();
            payloadConverterResolverMock.Setup(
                pcr => pcr.ResolveConverter(It.IsAny<string>()))
                .Returns(payloadConverterMock.Object);

            Mock<IEndpoint> endpointMock = new Mock<IEndpoint>();
            Mock<IRabbitChannel> rabbitChannelMock = new Mock<IRabbitChannel>();

            Mock<IRabbitConnection> rabbitConnectionMock = new Mock<IRabbitConnection>();
            rabbitConnectionMock
                .Setup(rcm => rcm.OpenChannel(It.IsAny<CancellationToken>()))
                .Returns(rabbitChannelMock.Object);

            Mock<IRouteResolver> routeResolverMock = new Mock<IRouteResolver>();

            Mock<IMessage> messageMock = new Mock<IMessage>();
            messageMock.SetupGet(m => m.Headers)
                .Returns(new Dictionary<string, object>());


            var sut = new Producer(
                endpointMock.Object,
                rabbitConnectionMock.Object,
                "message".ToMessageLabel(),
                routeResolverMock.Object,
                true,
                payloadConverterResolverMock.Object
            );
            sut.Start();

            sut.Publish(messageMock.Object);

            rabbitChannelMock.Verify(
                pc => pc.Publish(
                    It.IsAny<IRoute>(), 
                    It.IsAny<IMessage>(), 
                    It.Is<IPayloadConverter>(c => c == payloadConverterMock.Object)), 
                Times.Once);
        }

        private Mock<IPayloadConverter> CreateConverter(string contentType)
        {
            var converterMock = new Mock<IPayloadConverter>();
            converterMock.SetupGet(c => c.ContentType).Returns(contentType);
            converterMock.Setup(c => c.FromObject(It.IsAny<object>()))
                .Returns(System.Text.Encoding.UTF8.GetBytes(contentType));
            return converterMock;
        }
    }
}