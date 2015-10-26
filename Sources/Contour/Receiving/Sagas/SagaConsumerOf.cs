using System;

using Contour.Receiving.Consumers;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Получатель сообщения, которое участвует в саге.
    /// </summary>
    /// <typeparam name="TS">Тип данных для хранения временных данных - состояния саги.</typeparam>
    /// <typeparam name="TM">Тип входящего сообщения, которое участвует в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class SagaConsumerOf<TS, TM, TK> : IConsumerOf<TM>
        where TM : class
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SagaConsumerOf{TS,TM,TK}"/>. 
        /// </summary>
        /// <param name="sagaLifecycle">Управляет жизненным циклом саги.</param>
        /// <param name="sagaStep">Обработчик шага саги.</param>
        /// <param name="canInitiate">Если <c>true</c> - тогда сага может быть создана обработчиком сообщения.</param>
        /// <param name="failedHandler">Обработчик ошибок в саге.</param>
        public SagaConsumerOf(ISagaLifecycle<TS, TM, TK> sagaLifecycle, ISagaStep<TS, TM, TK> sagaStep, bool canInitiate, ISagaFailedHandler<TS, TM, TK> failedHandler)
        {
            this.SagaLifecycle = sagaLifecycle;
            this.SagaStep = sagaStep;
            this.CanInitiate = canInitiate;
            this.SagaFailedHandler = failedHandler;
        }

        /// <summary>
        /// Признак возможности создания саги на этом шаге.
        /// Если <c>false</c>, тогда шаг не начальный и создавать сагу нельзя, иначе <c>true</c>.
        /// </summary>
        public bool CanInitiate { get; internal set; }

        /// <summary>
        /// Выполняемое действие на этом шаге.
        /// </summary>
        public ISagaStep<TS, TM, TK> SagaStep { get; internal set; }

        /// <summary>
        /// Обработчик ошибок при обработке сообщений.
        /// </summary>
        public ISagaFailedHandler<TS, TM, TK> SagaFailedHandler { get; internal set; }

        /// <summary>
        /// Жизненный цикл саги.
        /// </summary>
        public ISagaLifecycle<TS, TM, TK> SagaLifecycle { get; internal set; }

        /// <summary>
        /// Обрабатывает входящее сообщение.
        /// </summary>
        /// <param name="context">Контекст обработки входящего сообщения.</param>
        public void Handle(IConsumingContext<TM> context)
        {
            var saga = this.SagaLifecycle.InitializeSaga(context, this.CanInitiate);

            if (saga == null)
            {
                this.SagaFailedHandler.SagaNotFoundHandle(context);

                return;
            }

            try
            {
                this.SagaStep.Handle(saga, context);
            }
            catch (Exception exception)
            {
                this.SagaFailedHandler.SagaFailedHandle(saga, context, exception);
                throw;
            }

            this.SagaLifecycle.FinilizeSaga(saga);
        }
    }
}
