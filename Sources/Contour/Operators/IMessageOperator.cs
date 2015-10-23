namespace Contour.Operators
{
    using System.Collections.Generic;

    /// <summary>
    /// Message operator.
    /// </summary>
    public interface IMessageOperator
    {
        /// <summary>
        /// Processes incoming message optionally producing stream of messages.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Stream of produced messages.</returns>
        IEnumerable<IMessage> Apply(IMessage message);
    }
}
