using System;

using Contour.Receiving;
using Contour.Receiving.Sagas;

using Moq;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Набор тестов для котроллера времени жизни саги.
    /// </summary>
    public class DefaultSagaLifecycleSpecs
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
            public void ShouldLoadSagaDataFromRepositoryBySagaId()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var sagaRepositoryMock = new Mock<ISagaRepository<Data, string>>();
                sagaRepositoryMock.Setup(sr => sr.Get(It.IsAny<string>()))
                    .Returns(sagaMock.Object);

                var sagaFactoryMock = new Mock<ISagaFactory<Data, string>>();

                var sagaIdSeparatorMock = new Mock<ISagaIdSeparator<IncomingEvent, string>>();

                var sut = new DefaultSagaLifecycle<Data, IncomingEvent, string>(sagaRepositoryMock.Object, sagaIdSeparatorMock.Object, sagaFactoryMock.Object);

                sut.InitializeSaga(consumingContextMock.Object, false);

                sagaRepositoryMock.Verify(sr => sr.Get(It.IsAny<string>()), "Должна быть получена сага из хранилища.");
            }

            /// <summary>
            /// Сага должна сохраняться в хранилище.
            /// </summary>
            [Test]
            public void ShouldStoreSagaAfterHandle()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var sagaRepositoryMock = new Mock<ISagaRepository<Data, string>>();
                sagaRepositoryMock.Setup(sr => sr.Get(It.IsAny<string>()))
                    .Returns(sagaMock.Object);

                var sagaFactoryMock = new Mock<ISagaFactory<Data, string>>();

                var sagaIdSeparatorMock = new Mock<ISagaIdSeparator<IncomingEvent, string>>();

                var sut = new DefaultSagaLifecycle<Data, IncomingEvent, string>(sagaRepositoryMock.Object, sagaIdSeparatorMock.Object, sagaFactoryMock.Object);

                sut.FinilizeSaga(sagaMock.Object);

                sagaRepositoryMock.Verify(sr => sr.Store(It.IsAny<ISagaContext<Data, string>>()), "Сага должна сохраняться после обработки.");
            }

            /// <summary>
            /// Сага должна быть создана.
            /// </summary>
            [Test]
            public void ShouldCreateSagaIfNotExist()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaMock = new Mock<ISagaContext<Data, string>>();

                var sagaRepositoryMock = new Mock<ISagaRepository<Data, string>>();
                sagaRepositoryMock.Setup(sr => sr.Get(It.IsAny<string>()))
                    .Returns((ISagaContext<Data, string>)null);

                var sagaFactoryMock = new Mock<ISagaFactory<Data, string>>();
                sagaFactoryMock.Setup(sf => sf.Create(It.IsAny<string>()))
                    .Returns(sagaMock.Object);

                var sagaIdSeparatorMock = new Mock<ISagaIdSeparator<IncomingEvent, string>>();

                var sut = new DefaultSagaLifecycle<Data, IncomingEvent, string>(sagaRepositoryMock.Object, sagaIdSeparatorMock.Object, sagaFactoryMock.Object);

                sut.InitializeSaga(consumingContextMock.Object, true);

                sagaFactoryMock.Verify(sf => sf.Create(It.IsAny<string>()), "Сага должна быть создана в случае ее отсутствия.");
            }

            /// <summary>
            /// Завершенная сага должна быть удалена из хранилища.
            /// </summary>
            [Test]
            public void ShouldRemoveSagaIfCompleted()
            {
                var consumingContextMock = new Mock<IConsumingContext<IncomingEvent>>();
                consumingContextMock.SetupGet(cc => cc.Message)
                    .Returns(new Message<IncomingEvent>(string.Empty.ToMessageLabel(), new IncomingEvent()));

                var sagaMock = new Mock<ISagaContext<Data, string>>();
                sagaMock.SetupGet(s => s.Completed).Returns(true);

                var sagaRepositoryMock = new Mock<ISagaRepository<Data, string>>();
                sagaRepositoryMock.Setup(sr => sr.Get(It.IsAny<string>()))
                    .Returns(sagaMock.Object);

                var sagaFactoryMock = new Mock<ISagaFactory<Data, string>>();
                sagaFactoryMock.Setup(sf => sf.Create(It.IsAny<string>()))
                    .Returns(sagaMock.Object);

                var sagaIdSeparatorMock = new Mock<ISagaIdSeparator<IncomingEvent, string>>();

                var sut = new DefaultSagaLifecycle<Data, IncomingEvent, string>(sagaRepositoryMock.Object, sagaIdSeparatorMock.Object, sagaFactoryMock.Object);

                sut.FinilizeSaga(sagaMock.Object);

                sagaRepositoryMock.Verify(sr => sr.Remove(It.IsAny<ISagaContext<Data, string>>()), "Сага должна быть удалена после завершения.");
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
