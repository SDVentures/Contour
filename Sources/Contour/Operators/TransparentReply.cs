using System.Collections.Generic;

namespace Contour.Operators
{
    /// <summary>
    /// Оператор, который отсылает ответ на запрос и передает сообщение дальше.
    /// </summary>
    public class TransparentReply : IMessageOperator
    {
        /// <summary>
        /// Отвечает на запрос и передает сообщение дальше.
        /// </summary>
        /// <param name="message">Ответное сообщение.</param>
        /// <returns>Ответное сообщение без заголовка <c>ReplyRoute</c>.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            message.Headers.Remove(Headers.ReplyRoute);
            BusProcessingContext.Current.Delivery.ReplyWith(message);
            yield return message;
        }
    }
}
