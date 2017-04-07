using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RoundRobinSelector>();
        private readonly ConcurrentQueue<IProducer> producers;
        private IEnumerator<IProducer> enumerator;

        public RoundRobinSelector(ConcurrentQueue<IProducer> items)
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

            this.producers = items;
            this.enumerator = this.producers.GetEnumerator();
        }

        public IProducer Next()
        {
            while (true)
            {
                if (this.enumerator.MoveNext())
                {
                    return this.enumerator.Current;
                }

                Logger.Trace("Starting the next round of producers' selection");
                this.enumerator = this.producers.GetEnumerator();
            }
        }

        public IProducer Next(IMessage message)
        {
            return this.Next();
        }
    }
}