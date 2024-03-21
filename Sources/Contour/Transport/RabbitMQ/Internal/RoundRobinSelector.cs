using System;
using System.Collections.Generic;
using System.Linq;

using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Using round-robin algorithm to select the next producer. 
    /// Improved version: <see cref="RoundRobinGoodConditionProducerSelector"/>
    /// </summary>
    internal class RoundRobinSelector : IProducerSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RoundRobinSelector>();
        private readonly object syncRoot = new object();
        private readonly IEnumerable<IProducer> producers;
        private IEnumerator<IProducer> enumerator;

        public RoundRobinSelector(IEnumerable<IProducer> items)
        {
            this.producers = items ?? throw new ArgumentNullException(nameof(items));
            this.enumerator = this.producers.GetEnumerator();
        }

        public IProducer Next()
        {
            return this.NextInternal();
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

                while (true)
                {
                    if (this.enumerator.MoveNext())
                    {
                        return this.enumerator.Current;
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
