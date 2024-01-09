using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinCustomSelector : IProducerSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RoundRobinSelector>();
        private readonly object syncRoot = new object();
        private readonly IEnumerable<IProducer> producers;
        private IEnumerator<IProducer> enumerator;

        public RoundRobinCustomSelector(IEnumerable<IProducer> items)
        {
            this.producers = items ?? throw new ArgumentNullException(nameof(items));
            this.enumerator = this.producers.GetEnumerator();
        }

        public IProducer Next()
        {
            return NextInternal();
        }

        public IProducer PickByConnectionKey(string key)
        {
            var result = this.producers.FirstOrDefault(x => x.ConnectionKey == key);

            if (result == null)
            {
                throw new ArgumentException($"Unable to take the producer for connection key {key}");
            }

            return result;
        }

        private IProducer NextInternal()
        {
            lock (this.syncRoot)
            {
                var freshCycle = false;
                IProducer firstTakenConsumer = null;

                while (true)
                {
                    while (this.enumerator.MoveNext())
                    {
                        var current = this.enumerator.Current;
                        if (firstTakenConsumer == null)
                            firstTakenConsumer = current;
                        if (current.IsInGoodCondition)
                        {
                            return current;
                        }
                    }

                    if (freshCycle)
                    {
                        while (this.enumerator.MoveNext())
                        {
                            var current = this.enumerator.Current;
                            if (firstTakenConsumer == current)
                            {
                                return firstTakenConsumer;
                            }
                        }

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
