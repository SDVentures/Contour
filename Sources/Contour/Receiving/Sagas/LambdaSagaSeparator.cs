using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Вычленяет идентификатор саги с помощью анонимного метода.
    /// Переходник между анонимным методом и классом.
    /// </summary>
    /// <typeparam name="TM">Тип передаваемого сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class LambdaSagaSeparator<TM, TK> : ISagaIdSeparator<TM, TK>
        where TM : class
    {
        private readonly Func<Message<TM>, TK> separator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaSagaSeparator{TM,TK}"/>. 
        /// </summary>
        /// <param name="separator">Анонимный метод получения идентификатора саги на основе входящего сообщения.</param>
        public LambdaSagaSeparator(Func<Message<TM>, TK> separator)
        {
            this.separator = separator;
        }

        /// <summary>
        /// Вычисляет идентификатор саги на основе входящего сообщения.
        /// </summary>
        /// <param name="message">Сообщение, в котором находится идентификатор саги.</param>
        /// <returns>Идентификатор саги. </returns>
        public TK GetId(Message<TM> message)
        {
            return this.separator(message);
        }
    }
}
