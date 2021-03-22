using System;
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
            this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
            this.attempts = maxAttempts;
        }

        public Task<MessageExchange> Send(MessageExchange exchange, string connectionKey)
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
                    var producer = connectionKey == null ? this.selector.Next() : this.selector.PickByConnectionKey(connectionKey);
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
        /// Âûïîëíÿåò îïðåäåëÿåìûå ïðèëîæåíèåì çàäà÷è, ñâÿçàííûå ñ óäàëåíèåì, âûñâîáîæäåíèåì èëè ñáðîñîì íåóïðàâëÿåìûõ ðåñóðñîâ.
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