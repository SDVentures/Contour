using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class DefaultProducerSelectorBuilder : IProducerSelectorBuilder
    {
        public IProducerSelector Build(IEnumerable<IProducer> items)
        {
            return new RoundRobinSelector(items);
        }
    }
}