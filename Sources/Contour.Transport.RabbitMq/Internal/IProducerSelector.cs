namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// Describes a producer selector used by the Rabbit Sender to choose a suitable producer. Selector can use a specific algorithm which may depend on message headers, payload or any other parameters.
    /// </summary>
    internal interface IProducerSelector
    {
        /// <summary>
        /// Gets the next producer in the list
        /// </summary>
        /// <returns>
        /// The selected producer
        /// </returns>
        IProducer Next();

        /// <summary>
        /// Gets the next producer in the list using <paramref name="message"/> in the selection algorithm
        /// </summary>
        /// <param name="message">A message to be sent by the producer</param>
        /// <returns>The selected producer</returns>
        IProducer Next(IMessage message);
    }
}