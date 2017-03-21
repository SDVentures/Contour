using System.Collections;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private readonly IEnumerator enumerator;

        public RoundRobinSelector(IEnumerable items)
        {
            this.enumerator = items.GetEnumerator();
        }

        public TProducer Next<TProducer>()
        {
            while (true)
            {
                if (this.enumerator.MoveNext())
                {
                    return (TProducer)this.enumerator.Current;
                }

                this.enumerator.Reset();
            }
        }
    }
}