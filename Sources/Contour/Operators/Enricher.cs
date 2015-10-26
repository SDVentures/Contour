namespace Contour.Operators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Enricher operator.
    /// </summary>
    public class Enricher : IMessageOperator
    {
        private readonly Action<IMessage> enricherAction;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="enricherAction">Enricher function.</param>
        public Enricher(Action<IMessage> enricherAction)
        {
            this.enricherAction = enricherAction;
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            this.enricherAction(message);

            yield return message;
        }
    }
}
