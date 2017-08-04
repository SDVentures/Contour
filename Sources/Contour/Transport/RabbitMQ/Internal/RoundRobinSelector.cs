using System;
using System.Collections;
using System.Collections.Generic;

using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RoundRobinSelector>();
        private readonly object syncRoot = new object();
        private readonly IEnumerable producers;
        private IEnumerator enumerator;

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

            this.producers = items;
            this.enumerator = this.producers.GetEnumerator();
        }

        public TProducer Next<TProducer>()
        {
            return this.NextInternal<TProducer>();
        }

        public TProducer Next<TProducer>(IMessage message)
        {
            return this.NextInternal<TProducer>();
        }

        private TProducer NextInternal<TProducer>()
        {
            lock (this.syncRoot)
            {
                var freshCycle = false;

                while (true)
                {
                    if (this.enumerator.MoveNext())
                    {
                        return (TProducer)this.enumerator.Current;
                    }

                    if (freshCycle)
                    {
                        throw new Exception("Unable to take the next producer because no available producers left");
                    }

                    Logger.Trace("Starting the next round of producers' selection");
                    freshCycle = true;

                    this.enumerator = this.producers.GetEnumerator();
                }
            }
        }
    }
}