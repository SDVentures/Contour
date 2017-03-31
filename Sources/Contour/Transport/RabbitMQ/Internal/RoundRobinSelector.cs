using System;
using System.Collections.Generic;
using System.Linq;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private readonly IEnumerator<IProducer> enumerator;

        public RoundRobinSelector(IEnumerable<IProducer> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var list = items.ToList();
            if (!list.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(items));
            }

            this.enumerator = list.GetEnumerator();
        }

        public IProducer Next()
        {
            while (true)
            {
                if (this.enumerator.MoveNext())
                {
                    return this.enumerator.Current;
                }

                this.enumerator.Reset();
            }
        }

        public IProducer Next(IMessage message)
        {
            return this.Next();
        }
    }
}