using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal sealed class FaultTolerantProducer : IFaultTolerantProducer
    {
        private readonly ILog logger;
        private readonly ConcurrentDictionary<int, int> delays = new ConcurrentDictionary<int, int>();
        private readonly IProducerSelector selector;
        private readonly int maxAttempts;
        private readonly int maxRetryDelay;
        private readonly int inactivityResetDelay;
        private readonly Timer resetTimer;

        private DateTime lastOperationTime = DateTime.Now;
        private bool disposed;
        
        public FaultTolerantProducer(IProducerSelector selector, int maxAttempts, int maxRetryDelay, int inactivityResetDelay)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.GetHashCode()})");

            this.selector = selector;
            this.maxAttempts = maxAttempts;
            this.maxRetryDelay = maxRetryDelay;
            this.inactivityResetDelay = inactivityResetDelay;

            this.resetTimer = new Timer(this.OnTimer);

            this.logger.Trace(
                $"Initialized with max attempts = {this.maxAttempts}, max retry delay = {this.maxRetryDelay}, inactivity reset delay = {this.inactivityResetDelay}");
        }

        public IEnumerable<KeyValuePair<int, int>> Delays => this.delays;

        public Task<MessageExchange> Send(MessageExchange exchange)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(typeof(FaultTolerantProducer).Name);
            }

            var errors = new List<Exception>();

            for (var count = 0; count < this.maxAttempts; count++)
            {
                this.logger.Trace($"Attempt to send #{count}");

                try
                {
                    var producer = this.selector.Next();
                    var hash = producer.GetHashCode();
                    var delay = this.delays.GetOrAdd(hash, 0);
                    this.logger.Debug($"Producer [{hash}] delay is {delay} seconds");

                    try
                    {
                        var tcs = new TaskCompletionSource<MessageExchange>();
                        var task = tcs.Task.ContinueWith(t =>
                        {
                            var sendTask = this.TrySend(exchange, producer);
                            this.logger.Debug($"Producer [{hash}] send operation has been scheduled");
                            return sendTask;
                        });

                        task.ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                throw new Exception($"Producer [{hash}] has failed", t.Exception);
                            }

                            this.ResetDelay(hash);
                            this.logger.Debug($"Producer [{hash}] delay has been reset");
                        });

                        new Timer(_ => tcs
                            .SetResult(null))
                            .Change((int)TimeSpan.FromSeconds(delay).TotalMilliseconds, -1);

                        return task.Result;
                    }
                    catch (Exception)
                    {
                        this.IncreaseDelay(hash, delay);
                        this.logger.Warn($"Producer [{hash}] delay has been set to {this.delays[hash]} seconds");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Warn($"Attempt #{count} to send a message has failed", ex);
                    errors.Add(ex);
                }
                finally
                {
                    this.lastOperationTime = DateTime.Now;
                    this.logger.Debug($"Last operation time is [{this.lastOperationTime}]");

                    var dueTime = TimeSpan.FromSeconds(this.inactivityResetDelay);
                    this.logger.Debug($"Delays reset due time is [{dueTime}]");

                    this.resetTimer.Change((int)dueTime.TotalMilliseconds, -1);
                }
            }

            throw new FailoverException($"Failed to send a message after {this.maxAttempts} attempts", new AggregateException(errors))
            {
                Attempts = this.maxAttempts
            };
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.resetTimer?.Dispose();
            this.disposed = true;
        }

        private void ResetDelay(int hash)
        {
            this.delays.AddOrUpdate(hash, 0, (h, previousDelay) => 0);
        }

        private void IncreaseDelay(int hash, int delay)
        {
            this.delays.AddOrUpdate(
                hash,
                delay,
                (h, previousDelay) => Math.Min(2 * (previousDelay + 1), this.maxRetryDelay));
        }

        private void OnTimer(object state)
        {
            this.logger.Trace($"Producers' delays will be reset because none of them has been active since [{this.lastOperationTime}]");
            this.delays.Clear();
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
    }
}
