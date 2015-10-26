namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Вычисляет идентификатор саги на основе заголовка <c>x-correlation-id</c> входящего сообщения.
    /// </summary>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class CorrelationIdSeparator<TM, TK> : ISagaIdSeparator<TM, TK>
        where TM : class
    {
        /// <summary>
        /// Вычисляет идентификатор саги на основе входящего сообщения.
        /// </summary>
        /// <param name="message">Сообщение, в котором находится идентификатор саги.</param>
        /// <returns>Идентификатор саги.</returns>
        public TK GetId(Message<TM> message)
        {
            return Headers.Extract<TK>(message.Headers, Headers.CorrelationId);
        }
    }
}
