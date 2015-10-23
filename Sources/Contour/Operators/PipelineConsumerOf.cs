namespace Contour.Operators
{
    /// <summary>
    /// Pipeline operator based consumer.
    /// </summary>
    /// <typeparam name="T">Payload type</typeparam>
    public class PipelineConsumerOf<T> : OperatorConsumerOf<T>
        where T : class
    {
        /// <summary>
        /// Builds consumer.
        /// </summary>
        /// <param name="operators">Operators list.</param>
        public PipelineConsumerOf(params IMessageOperator[] operators)
            : base(new Pipeline(operators))
        {
        }
    }
}
