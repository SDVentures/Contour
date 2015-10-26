namespace Contour.Operators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Splitter operator.
    /// </summary>
    public class Splitter : IMessageOperator
    {
        private readonly Func<IMessage, IEnumerable<object>> splitterFunc;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="splitterFunc">Splitter function.</param>
        public Splitter(Func<IMessage, IEnumerable<object>> splitterFunc)
        {
            this.splitterFunc = splitterFunc;
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            // TODO: define context
            return this.splitterFunc(message).Select(message.WithPayload);
        }
    }
}
