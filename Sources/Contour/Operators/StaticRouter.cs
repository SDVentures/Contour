namespace Contour.Operators
{
    using System.Collections.Generic;

    /// <summary>
    /// Static router operator.
    /// </summary>
    public class StaticRouter : IMessageOperator
    {
        private readonly MessageLabel label;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="label">Target label.</param>
        public StaticRouter(MessageLabel label)
        {
            this.label = label;
        }

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="label">Target label.</param>
        public StaticRouter(string label)
            : this(label.ToMessageLabel())
        {
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            yield return message.WithLabel(this.label);
        }
    }
}
