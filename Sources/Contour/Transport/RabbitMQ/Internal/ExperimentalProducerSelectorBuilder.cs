using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class ExperimentalProducerSelectorBuilder : IProducerSelectorBuilder
    {
        public IProducerSelector Build(IEnumerable<IProducer> items)
        {
            return new RoundRobinGoodConditionProducerSelector(items);
        }
    }
}
