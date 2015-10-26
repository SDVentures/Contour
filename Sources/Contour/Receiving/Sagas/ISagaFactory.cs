namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Фабрика саги.
    /// </summary>
    /// <typeparam name="TD">Тип состояния саги.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaFactory<TD, TK>
    {
        /// <summary>
        /// Создает сагу на основе переданного идентификатора.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <returns>Созданная сага.</returns>
        ISagaContext<TD, TK> Create(TK sagaId);

        /// <summary>
        /// Создает сагу на основе идентификатора и состояния саги.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <param name="data">Состояние саги.</param>
        /// <returns>Созданная сага.</returns>
        ISagaContext<TD, TK> Create(TK sagaId, TD data);
    }
}
