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
        /// Возвращает IProducer с конкретным ключем подключения к шине
        /// </summary>
        /// <param name="key">ключ подключения</param>
        /// <returns>The selected producer</returns>
        IProducer PickByConnectionKey(string key);
    }
}