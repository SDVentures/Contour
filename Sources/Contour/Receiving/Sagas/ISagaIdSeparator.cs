namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Вычислитель идентификатора саги на основе входящего сообщения.
    /// </summary>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaIdSeparator<TM, TK>
        where TM : class
    {
        /// <summary>
        /// Вычисляет идентификатор саги на основе входящего сообщения.
        /// </summary>
        /// <param name="message">Сообщение, в котором находится идентификатор саги.</param>
        /// <returns>Идентификатор саги.</returns>
        TK GetId(Message<TM> message);
    }
}
