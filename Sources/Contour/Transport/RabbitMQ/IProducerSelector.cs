namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// Describes a producer selector used by the Rabbit Sender to choose a suitable producer. Selector can use a specific algorithm which may depend on message headers, payload or any other parameters.
    /// </summary>
    public interface IProducerSelector
    {
        /// <summary>
        /// Gets the next producer in the list
        /// </summary>
        /// <typeparam name="TProducer">
        /// The type of the producer
        /// </typeparam>
        /// <returns>
        /// The selected producer
        /// </returns>
        TProducer Next<TProducer>();

        /// <summary>
        /// Gets the next producer in the list using <paramref name="message"/> in the selection algorithm
        /// </summary>
        /// <param name="message">A message to be sent by the producer</param>
        /// <typeparam name="TProducer">The type of the producer</typeparam>
        /// <returns>The selected producer</returns>
        TProducer Next<TProducer>(IMessage message);
    }
}