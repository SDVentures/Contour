using System.Collections;

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// Defines a builder for a producer selector
    /// </summary>
    internal interface IProducerSelectorBuilder
    {
        /// <summary>
        /// Builds a new <see cref="IProducerSelector"/>
        /// </summary>
        /// <param name="items">
        /// An optional set of producers to initialize the selector
        /// </param>
        /// <returns>
        /// The <see cref="IProducerSelector"/>.
        /// </returns>
        IProducerSelector Build(IList items = null);
    }
}