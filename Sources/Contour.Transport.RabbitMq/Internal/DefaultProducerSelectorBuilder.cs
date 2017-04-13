using System.Collections.Concurrent;

namespace Contour.Transport.RabbitMq.Internal
{
    internal class DefaultProducerSelectorBuilder : IProducerSelectorBuilder
    {
        public IProducerSelector Build(ConcurrentQueue<IProducer> items)
        {
            return new RoundRobinSelector(items);
        }
    }
}