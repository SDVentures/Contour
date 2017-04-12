namespace Contour.Receiving
{
    using Contour.Configuration;
    using Contour.Receiving.Consumers;

    /// <summary>
    /// The Receiver interface.
    /// </summary>
    public interface IReceiver : IBusComponent
    {
        /// <summary>
        /// The register consumer.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="consumer">
        /// The consumer.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer) where T : class;
    }
}
