namespace Contour.Receiving.Consumers
{
    /// <summary>
    /// The ConsumerOf interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IConsumerOf<T> : IConsumer
        where T : class
    {
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void Handle(IConsumingContext<T> context);
    }
}
