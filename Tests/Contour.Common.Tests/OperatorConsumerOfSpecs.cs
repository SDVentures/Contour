using System.Collections.Generic;
using System.Linq;
using Contour.Operators;
using Contour.Receiving;

using Moq;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Set of tests for <see cref="OperatorConsumerOf{T}"/>
    /// </summary>
    public class OperatorConsumerOfSpecs
    {
        /// <summary>
        /// When consuming message
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenConsumeMessage
        {
            /// <summary>
            /// Should attach <c>Breadcrumbs</c> header. 
            /// </summary>
            [Test]
            public void ShouldAttachBreadcrumb()
            {
                var address = "test";

                var messageOperatorMock = new Mock<IMessageOperator>();
                messageOperatorMock.Setup(mo => mo.Apply(It.IsAny<IMessage>()))
                    .Returns<IMessage>(
                        m => new List<IMessage>
                                 {
                                     m
                                 });

                var busContextMock = new Mock<IBusContext>();
                busContextMock.Setup(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()));
                busContextMock.SetupGet(bc => bc.Endpoint)
                    .Returns(new Endpoint(address));

                var deliveryMock = new Mock<IDelivery>();

                var consumingContextMock = new Mock<IConsumingContext<object>>();
                consumingContextMock.As<IDeliveryContext>()
                    .SetupGet(cc => cc.Delivery)
                    .Returns(deliveryMock.Object);
                consumingContextMock.SetupGet(cc => cc.Bus)
                    .Returns(busContextMock.Object);
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<object>("label".ToMessageLabel(), new Dictionary<string, object>(), new object()));

                var sut = new OperatorConsumerOf<object>(messageOperatorMock.Object);
                sut.Handle(consumingContextMock.Object);

                busContextMock.Verify(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.Is<IDictionary<string, object>>(value => value.ContainsKey(Headers.Breadcrumbs) && Headers.GetString(value, Headers.Breadcrumbs) == address)), Times.Once, "Должен быть установлен заголовок Headers.Breadcrumbs");
            }

            /// <summary>
            /// Should set <c>OriginalMessageId</c> header.
            /// </summary>
            [Test]
            public void ShouldAttachCorrelationId()
            {
                var messageOperatorMock = new Mock<IMessageOperator>();
                messageOperatorMock.Setup(mo => mo.Apply(It.IsAny<IMessage>()))
                    .Returns<IMessage>(
                        m => new List<IMessage>
                                 {
                                     m
                                 });

                var busContextMock = new Mock<IBusContext>();
                busContextMock.Setup(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()));
                busContextMock.SetupGet(bc => bc.Endpoint)
                    .Returns(new Endpoint(string.Empty));

                var deliveryMock = new Mock<IDelivery>();

                var consumingContextMock = new Mock<IConsumingContext<object>>();
                consumingContextMock.As<IDeliveryContext>()
                    .SetupGet(cc => cc.Delivery)
                    .Returns(deliveryMock.Object);
                consumingContextMock.SetupGet(cc => cc.Bus)
                    .Returns(busContextMock.Object);
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<object>("label".ToMessageLabel(), new Dictionary<string, object>(), new object()));

                var sut = new OperatorConsumerOf<object>(messageOperatorMock.Object);
                sut.Handle(consumingContextMock.Object);

                busContextMock.Verify(bc => bc.Emit(It.IsAny<MessageLabel>(), It.IsAny<object>(), It.Is<IDictionary<string, object>>(value => value.ContainsKey(Headers.OriginalMessageId) && !string.IsNullOrEmpty(Headers.GetString(value, Headers.OriginalMessageId)))), Times.Once, "Должен быть установлен заголовок Headers.CorrelationId");
            }

            /// <summary>
            /// should set BusProcessingContext.
            /// </summary>
            [Test]
            public void ShouldSetBusProcessingContext()
            {
                var messageOperatorMock = new Mock<IMessageOperator>();

                IDelivery deliveryFromProcessingContext = null;
                IBusContext busContextFromProcessingContext = null;
                messageOperatorMock.Setup(mo => mo.Apply(It.IsAny<IMessage>()))
                    .Returns<IMessage>(m => Enumerable.Empty<IMessage>())
                    .Callback(
                        (IMessage m) =>
                            {
                                deliveryFromProcessingContext = BusProcessingContext.Current.Delivery;
                                busContextFromProcessingContext = BusProcessingContext.Current.BusContext;
                            });

                var busContextMock = new Mock<IBusContext>();
                var deliveryMock = new Mock<IDelivery>();

                var consumingContextMock = new Mock<IConsumingContext<object>>();
                consumingContextMock.As<IDeliveryContext>()
                    .SetupGet(cc => cc.Delivery)
                    .Returns(deliveryMock.Object);
                consumingContextMock.SetupGet(cc => cc.Bus)
                    .Returns(busContextMock.Object);
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<object>("label".ToMessageLabel(), new Dictionary<string, object>(), new object()));

                var sut = new OperatorConsumerOf<object>(messageOperatorMock.Object);
                sut.Handle(consumingContextMock.Object);

                Assert.AreEqual(busContextMock.Object, busContextFromProcessingContext);
                Assert.AreEqual(deliveryMock.Object, deliveryFromProcessingContext);
            }
        }
    }
}
