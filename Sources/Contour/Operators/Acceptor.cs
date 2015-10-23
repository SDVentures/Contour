using System.Collections.Generic;

namespace Contour.Operators
{
    /// <summary>
    /// Помечает сообщение как обработаное в случае, если нужно явное подтверждение обработки сообщения.
    /// После подтверждения сообщение передается дальше.
    /// </summary>
    public class Acceptor : IMessageOperator
    {
        /// <summary>
        /// Помечает сообщение как обработаное.
        /// </summary>
        /// <param name="message">Входящее сообщение, которое нужно поменить как обработанное.</param>
        /// <returns>Входящее сообщение, помеченное как обработанное.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            BusProcessingContext.Current.Delivery.Accept();
            yield return message;
        }
    }
}
