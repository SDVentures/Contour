using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Using round-robin algorithm to select the next producer. 
    /// Will skip producers in a bad state if there are any good producers left.
    /// </summary>
    internal class RoundRobinGoodConditionProducerSelector : IProducerSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RoundRobinGoodConditionProducerSelector>();
        private readonly object syncRoot = new object();
        private readonly IEnumerable<IProducer> producers;
        private IEnumerator<IProducer> enumerator;

        public RoundRobinGoodConditionProducerSelector(IEnumerable<IProducer> items)
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
                var secondCycle = false;
                IProducer firstTakenConsumer = null;

                while (true)
                {
                    while (this.enumerator.MoveNext())
                    {
                        var current = this.enumerator.Current;
                        if (current.IsInGoodCondition)
                        {
                            return current;
                        }
                        if (firstTakenConsumer == null)
                        { 
                            // save first consumer in case we can't find any in a good state
                            firstTakenConsumer = current;
                        }
                    }

                    if (secondCycle)
                    {
                        while (this.enumerator.MoveNext())
                        {
                            var current = this.enumerator.Current;
                            if (current.IsInGoodCondition)
                            {
                                return current;
                            }
                            if (firstTakenConsumer == current)
                            {
                                // assuming that we iterated a whole cycle and all consumers are not in a good state
                                // so return the first one
                                return firstTakenConsumer;
                            }
                        }

                        throw new Exception("Unable to take the next producer because no available producers left");
                    }

                    Logger.Trace("Starting the next round of producers' selection");
                    secondCycle = true;

                    this.enumerator = this.producers.GetEnumerator();
                }
            }
        }
    }
}
