namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Фабрика саги используемая по умолчанию.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class DefaultSagaFactory<TS, TK> : ISagaFactory<TS, TK>
    {
        /// <summary>
        /// Создает сагу на основе переданного идентификатора.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <returns>Созданная сага.</returns>
        public ISagaContext<TS, TK> Create(TK sagaId)
        {
            return new SagaContext<TS, TK>(sagaId);
        }

        /// <summary>
        /// Создает сагу на основе идентификатора и состояния саги.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <param name="data">Состояние саги.</param>
        /// <returns>Созданная сага.</returns>
        public ISagaContext<TS, TK> Create(TK sagaId, TS data)
        {
            return new SagaContext<TS, TK>(sagaId, data);
        }
    }
}
