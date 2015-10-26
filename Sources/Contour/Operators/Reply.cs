namespace Contour.Operators
{
    using System.Collections.Generic;

    /// <summary>
    /// Replying operator.
    /// </summary>
    public class Reply : IMessageOperator
    {
        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            message.Headers.Remove(Headers.ReplyRoute);
            BusProcessingContext.Current.Delivery.ReplyWith(message);
            yield break;
        }
    }
}
