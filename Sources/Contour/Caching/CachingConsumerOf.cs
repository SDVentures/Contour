using System;

using Common.Logging;

namespace Contour.Caching
{
    using Contour.Receiving;
    using Contour.Receiving.Consumers;

    public class CachingConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        private static readonly ILog Log = LogManager.GetLogger("CachingConsumerOf");

        private readonly IConsumerOf<T> consumer;

        private readonly CacheConfiguration cacheConfiguration;

        private IMetricsCollector metricsCollector;

        private bool metricsCollectorInitialized;

        public CachingConsumerOf(IConsumerOf<T> consumer, CacheConfiguration cacheConfiguration)
        {
            this.consumer = consumer;
            this.cacheConfiguration = cacheConfiguration;
        }

        public void Handle(IConsumingContext<T> context)
        {
            this.EnsureMetricCollector(context);

            var message = context.Message;

            if (!this.CanReplyAndExistsCacheConfig(context))
            {
                this.CollectMetrics(message.Label.ToString(), false);
                this.consumer.Handle(context);
                return;
            }

            var cachedValue = this.cacheConfiguration.Cache[message];
            var cached = cachedValue != null;

            this.CollectMetrics(message.Label.ToString(), cached);

            if (cached)
            {
                context.Reply(cachedValue);
                return;
            }

            var cachingContext = new CachingContext<T>(context, this.cacheConfiguration);

            this.consumer.Handle(cachingContext);
        }

        private void EnsureMetricCollector(IConsumingContext<T> context)
        {
            if (this.metricsCollectorInitialized)
            {
                return;
            }

            lock (this)
            {
                try
                {
                    this.metricsCollector = (context.Bus as AbstractBus)?.Configuration?.MetricsCollector;
                }
                catch (Exception e)
                {
                    Log.Warn(m => m("Could not get metric collector"), e);
                }

                this.metricsCollectorInitialized = true;
            }
        }

        private bool CanReplyAndExistsCacheConfig(IConsumingContext<T> context)
        {
            return context.CanReply && (this.cacheConfiguration?.Enabled ?? false) && this.cacheConfiguration?.Cache != null && this.cacheConfiguration?.Ttl != null;
        }

        private void CollectMetrics(string label, bool hit) => this.metricsCollector?.Increment("contour.incoming.cache_usage.count", 1d, new[] { "cache:" + (hit ? "hit" : "miss"), "label:" + label });
    }
}