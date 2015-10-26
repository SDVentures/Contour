namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Контролирует время жизни саги.
    /// </summary>
    /// <typeparam name="TS">Тип данных саги.</typeparam>
    /// <typeparam name="TM">Тип данных сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class DefaultSagaLifecycle<TS, TM, TK> : ISagaLifecycle<TS, TM, TK>
        where TM : class
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DefaultSagaLifecycle{TS,TM,TK}"/>. 
        /// </summary>
        /// <param name="repository">Хранилище временного состояния саги.</param>
        /// <param name="separator">Вычислитель идентификатора саги из сообщения.</param>
        /// <param name="factory">Фабрика создающая сагу.</param>
        public DefaultSagaLifecycle(ISagaRepository<TS, TK> repository, ISagaIdSeparator<TM, TK> separator, ISagaFactory<TS, TK> factory)
        {
            this.SagaRepository = repository;
            this.SagaIdSeparator = separator;
            this.SagaFactory = factory;
        }

        /// <summary>
        /// Хранилище временного состояния саги.
        /// </summary>
        public ISagaRepository<TS, TK> SagaRepository { get; internal set; }

        /// <summary>
        /// Вычислитель идентификатора саги из сообщения.
        /// </summary>
        public ISagaIdSeparator<TM, TK> SagaIdSeparator { get; internal set; }

        /// <summary>
        /// Фабрика создающая сагу.
        /// </summary>
        public ISagaFactory<TS, TK> SagaFactory { get; internal set; }

        /// <summary>
        /// Инициализирует сагу.
        /// </summary>
        /// <param name="context">Контекст обработки входящего сообщения.</param>
        /// <param name="canInitiate">Если <c>true</c> - тогда при инициализации можно создавать новую сагу.</param>
        /// <returns>Сага соответствующая сообщению, новая сага или <c>null</c>, если сагу получить или создать невозможно.</returns>
        public ISagaContext<TS, TK> InitializeSaga(IConsumingContext<TM> context, bool canInitiate)
        {
            var sagaId = this.SagaIdSeparator.GetId(context.Message);
            var saga = this.SagaRepository.Get(sagaId);

            if (saga == null)
            {
                if (canInitiate)
                {
                    saga = this.SagaFactory.Create(sagaId);
                }
            }

            return saga;
        }

        /// <summary>
        /// Завершает обработку саги. 
        /// </summary>
        /// <param name="sagaContext">Завершаемая сага.</param>
        public void FinilizeSaga(ISagaContext<TS, TK> sagaContext)
        {
            if (sagaContext.Completed)
            {
                this.SagaRepository.Remove(sagaContext);
            }
            else
            {
                this.SagaRepository.Store(sagaContext);
            }
        }
    }
}
