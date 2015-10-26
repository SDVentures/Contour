namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Контролирует время жизни саги.
    /// </summary>
    /// <typeparam name="TS">Тип данных саги.</typeparam>
    /// <typeparam name="TM">Тип данных сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaLifecycle<TS, TM, TK>
        where TM : class
    {
        /// <summary>
        /// Инициализирует сагу.
        /// </summary>
        /// <param name="context">Контекст обработки входящего сообщения.</param>
        /// <param name="canInitiate">Если <c>true</c> - тогда при инициализации можно создавать новую сагу.</param>
        /// <returns>Сага соответствующая сообщению, новая сага или <c>null</c>, если сагу получить или создать невозможно.</returns>
        ISagaContext<TS, TK> InitializeSaga(IConsumingContext<TM> context, bool canInitiate);

        /// <summary>
        /// Завершает обработку саги. 
        /// </summary>
        /// <param name="sagaContext">Завершаемая сага.</param>
        void FinilizeSaga(ISagaContext<TS, TK> sagaContext);
    }
}
