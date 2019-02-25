using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class FaultTolerantProducer : IFaultTolerantProducer
    {
        private readonly ILog logger = LogManager.GetLogger<FaultTolerantProducer>();
        private readonly IProducerSelector selector;
        private readonly int attempts;

        private bool disposed;

        public FaultTolerantProducer(IProducerSelector selector, int maxAttempts, int maxRetryDelay, int inactivityResetDelay)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            this.selector = selector;
            this.attempts = maxAttempts;
        }

        public IEnumerable<KeyValuePair<int, int>> Delays { get; } = new ConcurrentDictionary<int, int>();

        public Task<MessageExchange> Send(MessageExchange exchange)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(typeof(FaultTolerantProducer).Name);
            }

            var errors = new List<Exception>();

            for (var count = 0; count < this.attempts; count++)
            {
                this.logger.Trace($"Attempt to send #{count}");

                try
                {
                    var producer = this.selector.Next();
                    return this.TrySend(exchange, producer);
                }
                catch (Exception ex)
                {
                    this.logger.Warn($"Attempt #{count} to send a message has failed", ex);
                    errors.Add(ex);
                }
            }

            throw new FailoverException($"Failed to send a message after {this.attempts} attempts", new AggregateException(errors))
            {
                Attempts = this.attempts
            };
        }

        private Task<MessageExchange> TrySend(MessageExchange exchange, IProducer producer)
        {
            if (exchange.IsRequest)
            {
                return producer.Request(exchange.Out, exchange.ExpectedResponseType)
                    .ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted)
                            {
                                exchange.Exception = t.Exception;
                            }
                            else
                            {
                                exchange.In = t.Result;
                            }

                            return exchange;
                        });
            }

            return producer.Publish(exchange.Out)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            exchange.Exception = t.Exception;
                        }

                        return exchange;
                    });
        }

        /// <summary>
        /// ¬ыполн€ет определ€емые приложением задачи, св€занные с удалением, высвобождением или сбросом неуправл€емых ресурсов.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }
    }
}