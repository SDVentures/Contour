namespace Contour.Operators
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Pipeline operator.
    /// </summary>
    public class Pipeline : IMessageOperator
    {
        private readonly IList<IMessageOperator> operators = new IMessageOperator[0];

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="operators">Operators list.</param>
        public Pipeline(params IMessageOperator[] operators)
        {
            this.operators = operators.ToList();
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            IEnumerable<IMessage> messages = new List<IMessage> { message };

            foreach (var @operator in this.operators)
            {
                var current = @operator;
                var old = messages;
                messages = old.SelectMany(current.Apply);
            }

            return messages;
        }
    }
}
