namespace Contour.Transport.RabbitMQ.Internal
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
        /// Возвращает IProducer с конкретной строкой подключения к RMQ
        /// </summary>
        /// <param name="url">Строка подключения к RMQ</param>
        /// <returns>The selected producer</returns>
        IProducer PickByBrockerUrl(string url);
    }
}