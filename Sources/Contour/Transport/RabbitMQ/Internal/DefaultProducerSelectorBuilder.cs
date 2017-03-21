using System.Collections;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class DefaultProducerSelectorBuilder : IProducerSelectorBuilder
    {
        public IProducerSelector Build(IEnumerable items)
        {
            return new RoundRobinSelector(items);
        }
    }
}