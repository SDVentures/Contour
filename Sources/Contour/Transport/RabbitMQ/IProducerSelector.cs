using System.Collections;

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// Describes a producer selector used by the Rabbit Sender to choose a suitable producer. Selector can use a specific algorithm which may depend on message headers, payload or any other parameters.
    /// </summary>
    public interface IProducerSelector
    {
        /// <summary>
        /// Initializes the selector
        /// </summary>
        /// <param name="list">
        /// The list of producers
        /// </param>
        void Initialize(IList list);
        
        /// <summary>
        /// Gets the next producer in the list
        /// </summary>
        /// <returns>
        /// The selected producer
        /// </returns>
        int Next();
    }
}