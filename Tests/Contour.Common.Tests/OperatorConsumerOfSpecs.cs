using System.Collections.Generic;

using Contour.Operators;
using Contour.Receiving;

using Moq;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Набор тестов для оператора обработки сообщения.
    /// </summary>
    public class OperatorConsumerOfSpecs
    {
        /// <summary>
        /// При обработке входящего сообщения
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenConsumeMessage
        {
            /// <summary>
            /// Должен подставляться заголовок <c>Breadcrumbs</c>.
            /// </summary>
            [Test]
            public void ShouldAttachBreadcrumb()
            {
                var address = "test";

                var messageOperatorMock = new Mock<IMessageOperator>();
                messageOperatorMock.Setup(mo => mo.Apply(It.IsAny<IMessage>())).Returns<IMessage>(m => new List<IMessage> { m });

                var busContextMock = new Mock<IBusContext>();
                busContextMock.Setup(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()));
                busContextMock.SetupGet(bc => bc.Endpoint).Returns(new Endpoint(address));

                var deliveryMock = new Mock<IDelivery>();

                var consumingContextMock = new Mock<IConsumingContext<object>>();
                consumingContextMock.As<IDeliveryContext>().SetupGet(cc => cc.Delivery).Returns(deliveryMock.Object);
                consumingContextMock.SetupGet(cc => cc.Bus).Returns(busContextMock.Object);
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<object>("label".ToMessageLabel(), new Dictionary<string, object>(), new object()));

                var sut = new OperatorConsumerOf<object>(messageOperatorMock.Object);
                sut.Handle(consumingContextMock.Object);

                busContextMock
                    .Verify(
                        bc => bc.Emit(
                            It.IsAny<MessageLabel>(),
                            It.IsAny<object>(),
                            It.Is<IDictionary<string, object>>(
                                value => value.ContainsKey(Headers.Breadcrumbs)
                                    && Headers.GetString(value, Headers.Breadcrumbs) == address)),
                        Times.Once,
                        "Должен быть установлен заголовок Headers.Breadcrumbs");
            }

            /// <summary>
            /// Должен подставляться заголовок <c>OriginalMessageId</c>.
            /// </summary>
            [Test]
            public void ShouldAttachCorrelationId()
            {
                var messageOperatorMock = new Mock<IMessageOperator>();
                messageOperatorMock.Setup(mo => mo.Apply(It.IsAny<IMessage>())).Returns<IMessage>(m => new List<IMessage> { m });

                var busContextMock = new Mock<IBusContext>();
                busContextMock.Setup(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()));
                busContextMock.SetupGet(bc => bc.Endpoint).Returns(new Endpoint(string.Empty));

                var deliveryMock = new Mock<IDelivery>();

                var consumingContextMock = new Mock<IConsumingContext<object>>();
                consumingContextMock.As<IDeliveryContext>().SetupGet(cc => cc.Delivery).Returns(deliveryMock.Object);
                consumingContextMock.SetupGet(cc => cc.Bus).Returns(busContextMock.Object);
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<object>("label".ToMessageLabel(), new Dictionary<string, object>(), new object()));

                var sut = new OperatorConsumerOf<object>(messageOperatorMock.Object);
                sut.Handle(consumingContextMock.Object);

                busContextMock
                    .Verify(
                        bc => bc.Emit(
                            It.IsAny<MessageLabel>(),
                            It.IsAny<object>(),
                            It.Is<IDictionary<string, object>>(
                                value => value.ContainsKey(Headers.OriginalMessageId)
                                    && !string.IsNullOrEmpty(Headers.GetString(value, Headers.OriginalMessageId)))),
                        Times.Once,
                        "Должен быть установлен заголовок Headers.CorrelationId");
            }
        }
    }
}
