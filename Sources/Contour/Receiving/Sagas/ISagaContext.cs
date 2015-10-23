namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Экземпляр состояния процесса координируемого с помощью саги.
    /// </summary>
    /// <typeparam name="TD">Тип состояния саги.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaContext<TD, TK>
    {
        /// <summary>
        /// Идентификатор саги.
        /// </summary>
        TK SagaId { get; }

        /// <summary>
        /// Если <c>true</c> - тогда сага завершена, иначе - <c>false</c>.
        /// </summary>
        bool Completed { get; }

        /// <summary>
        /// Пользовательские данные сохраняемые в саге.
        /// </summary>
        TD Data { get; }

        /// <summary>
        /// Обновляет пользовательские данные сохраняемые в саге.
        /// </summary>
        /// <param name="data">Пользовательские данные сохраняемые в саге.</param>
        void UpdateData(TD data);

        /// <summary>
        /// Отмечает сагу как завершенную.
        /// </summary>
        void Complete();
    }
}
