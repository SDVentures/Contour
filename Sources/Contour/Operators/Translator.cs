namespace Contour.Operators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Translator operator.
    /// </summary>
    public class Translator : IMessageOperator
    {
        private readonly Func<IMessage, object> translationFunc;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="translationFunc">Translator function.</param>
        public Translator(Func<IMessage, object> translationFunc)
        {
            this.translationFunc = translationFunc;
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            yield return message.WithPayload(this.translationFunc(message));
        }
    }
}
