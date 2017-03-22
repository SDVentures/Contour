using System;
using System.Collections;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private readonly IEnumerator enumerator;

        public RoundRobinSelector(IList items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(items));
            }

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

        public TProducer Next<TProducer>(IMessage message)
        {
            return this.Next<TProducer>();
        }
    }
}