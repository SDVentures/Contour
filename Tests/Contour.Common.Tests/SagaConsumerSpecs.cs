using System;

using Contour.Receiving;
using Contour.Receiving.Sagas;

using Moq;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Набор тестов для получателя сообщения участника саги.
    /// </summary>
    public class SagaConsumerSpecs
    {
        /// <summary>
        /// Когда обрабатывается сообщение участник саги.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenHandleMessageOfSaga
        {
            /// <summary>
            /// Сага должна восстанавливаться из хранилища.
            /// </summary>
            [Test]
            public void ShouldHandleMessageIfSagaExist()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var sagaLifecycleMock = new Mock<ISagaLifecycle<Data, IncomingEvent, string>>();
                sagaLifecycleMock.Setup(sl => sl.InitializeSaga(It.IsAny<IConsumingContext<IncomingEvent>>(), It.IsAny<bool>()))
                    .Returns(sagaMock.Object);

                var sagaFailedHandlerMock = new Mock<ISagaFailedHandler<Data, IncomingEvent, string>>();

                var sagaStepMock = new Mock<ISagaStep<Data, IncomingEvent, string>>();

                var sut = new SagaConsumerOf<Data, IncomingEvent, string>(sagaLifecycleMock.Object, sagaStepMock.Object, false, sagaFailedHandlerMock.Object);

                sut.Handle(consumingContextMock.Object);

                sagaStepMock.Verify(ss => ss.Handle(It.IsAny<ISagaContext<Data, string>>(), It.IsAny<IConsumingContext<IncomingEvent>>()), "Должна быть вызвана обработка входящего сообщения.");
            }

            /// <summary>
            /// Если обработчик не может быть инициатором саги и сага не найдена, тогда вызывается обработчик не найденных саг.
            /// </summary>
            [Test]
            public void ShouldHandleSagaNotFound()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaLifecycleMock = new Mock<ISagaLifecycle<Data, IncomingEvent, string>>();
                sagaLifecycleMock.Setup(sl => sl.InitializeSaga(It.IsAny<IConsumingContext<IncomingEvent>>(), It.IsAny<bool>()))
                    .Returns((ISagaContext<Data, string>)null);

                var sagaFailedHandlerMock = new Mock<ISagaFailedHandler<Data, IncomingEvent, string>>();

                var sagaStepMock = new Mock<ISagaStep<Data, IncomingEvent, string>>();

                var sut = new SagaConsumerOf<Data, IncomingEvent, string>(sagaLifecycleMock.Object, sagaStepMock.Object, false, sagaFailedHandlerMock.Object);

                sut.Handle(consumingContextMock.Object);

                sagaFailedHandlerMock.Verify(
                    sfh => sfh.SagaNotFoundHandle(It.IsAny<IConsumingContext<IncomingEvent>>()), 
                    "Если сага не может быть создана и не найдена, должен быть вызван обработчик.");
            }

            /// <summary>
            /// Должен вызываться обработчик исключений сгенерированных при обработке сообщения.
            /// </summary>
            [Test]
            public void ShouldHandleException()
            {
                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaLifecycleMock = new Mock<ISagaLifecycle<Data, IncomingEvent, string>>();
                sagaLifecycleMock.Setup(sl => sl.InitializeSaga(It.IsAny<IConsumingContext<IncomingEvent>>(), It.IsAny<bool>()))
                    .Returns(sagaMock.Object);

                var sagaStepMock = new Mock<ISagaStep<Data, IncomingEvent, string>>();
                sagaStepMock
                    .Setup(ss => ss.Handle(It.IsAny<ISagaContext<Data, string>>(), It.IsAny<IConsumingContext<IncomingEvent>>()))
                    .Throws(new Exception());

                var sagaFailedHandlerMock = new Mock<ISagaFailedHandler<Data, IncomingEvent, string>>();

                var sut = new SagaConsumerOf<Data, IncomingEvent, string>(sagaLifecycleMock.Object, sagaStepMock.Object, false, sagaFailedHandlerMock.Object);

                try
                {
                    sut.Handle(consumingContextMock.Object);
                }
                    // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }

                sagaFailedHandlerMock.Verify(sfh => sfh.SagaFailedHandle(It.IsAny<ISagaContext<Data, string>>(), It.IsAny<IConsumingContext<IncomingEvent>>(), It.IsAny<Exception>()), Times.Once, "Должен быть вызван обработчик исключений.");
            }

            /// <summary>
            /// Исключение должно быть проброшенно дальше.
            /// </summary>
            [Test]
            public void ShouldRethrowException()
            {
                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaLifecycleMock = new Mock<ISagaLifecycle<Data, IncomingEvent, string>>();
                sagaLifecycleMock.Setup(sl => sl.InitializeSaga(It.IsAny<IConsumingContext<IncomingEvent>>(), It.IsAny<bool>()))
                    .Returns(sagaMock.Object);

                var sagaStepMock = new Mock<ISagaStep<Data, IncomingEvent, string>>();
                sagaStepMock
                    .Setup(ss => ss.Handle(It.IsAny<ISagaContext<Data, string>>(), It.IsAny<IConsumingContext<IncomingEvent>>()))
                    .Throws(new Exception());

                var sagaFailedHandlerMock = new Mock<ISagaFailedHandler<Data, IncomingEvent, string>>();

                var sut = new SagaConsumerOf<Data, IncomingEvent, string>(sagaLifecycleMock.Object, sagaStepMock.Object, false, sagaFailedHandlerMock.Object);

                Assert.Throws<Exception>(() => sut.Handle(consumingContextMock.Object), "Исключение должно быть проброшено из обработчика сообщения.");
            }
        }

        /// <summary>
        /// Данные сохраняемые в саге.
        /// </summary>
        public class Data
        {
            /// <summary>
            /// Время запуска саги.
            /// </summary>
            public DateTime StartTime { get; set; }
        }

        /// <summary>
        /// Входящее сообщение.
        /// </summary>
        public class IncomingEvent
        {
            /// <summary>
            /// Идентификатор сообщения.
            /// </summary>
            public int Id { get; set; }
        }
    }
}
