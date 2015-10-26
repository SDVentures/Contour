namespace Contour.Operators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Filter operator.
    /// </summary>
    public class Filter : IMessageOperator
    {
        private readonly Predicate<IMessage> predicateFunc;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="predicateFunc">Predicate function.</param>
        public Filter(Predicate<IMessage> predicateFunc)
        {
            this.predicateFunc = predicateFunc;
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            if (this.predicateFunc(message))
            {
                yield return message;
            }
        }
    }
}
