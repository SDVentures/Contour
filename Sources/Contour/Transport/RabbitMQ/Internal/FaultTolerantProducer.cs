using System;
using System.Threading.Tasks;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class FaultTolerantProducer
    {
        private readonly ILog logger = LogManager.GetCurrentClassLogger();
        private readonly IProducerSelector selector;
        private readonly int attempts;
        private int count;

        public FaultTolerantProducer(IProducerSelector selector, int attempts)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            this.selector = selector;
            this.attempts = attempts;
        }

        public Task<MessageExchange> Try(MessageExchange exchange)
        {
            this.logger.Trace($"Attempt to send #{this.count}");
            var producer = this.selector.Next<Producer>();

            return this.CreateSendTask(exchange, producer)
                .ContinueWith(t =>
                {
                    if (t.IsCompleted && !t.IsFaulted)
                    {
                        return t.Result;
                    }

                    if (t.IsFaulted && this.count < this.attempts)
                    {
                        this.count++;
                        return this.Try(exchange).Result;
                    }

                    throw new InvalidOperationException("Send operation failed", t.Exception);
                });
        }

        private Task<MessageExchange> CreateSendTask(MessageExchange exchange, Producer producer)
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
    }
}