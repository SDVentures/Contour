namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Хранилище состояния саги.
    /// </summary>
    /// <typeparam name="TD">Тип сохраняемого состояния саги.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaRepository<TD, TK>
    {
        /// <summary>
        /// Получает сохраненную сагу.
        /// </summary>
        /// <param name="sagaId">Идентификатор запрашиваемой саги.</param>
        /// <returns>Запрашиваемая сага если она существует, иначе <c>null</c>.</returns>
        ISagaContext<TD, TK> Get(TK sagaId);

        /// <summary>
        /// Сохраняет сагу.
        /// </summary>
        /// <param name="sagaContext">Сохраняемая сага.</param>
        void Store(ISagaContext<TD, TK> sagaContext);

        /// <summary>
        /// Удаляет сагу.
        /// </summary>
        /// <param name="sagaContext">Удаляемая сага.</param>
        void Remove(ISagaContext<TD, TK> sagaContext);
    }
}
