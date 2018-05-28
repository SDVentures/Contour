namespace Contour.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Contour.Filters;

    public class CachingFilterDecorator : IMessageExchangeFilterDecorator
    {
        private readonly IDictionary<string, CacheConfiguration> cacheConfiguration;

        private readonly IMetricsCollector metricsCollector;

        public CachingFilterDecorator(IDictionary<string, CacheConfiguration> cacheConfiguration, IMetricsCollector metricsCollector)
        {
            this.cacheConfiguration = cacheConfiguration;
            this.metricsCollector = metricsCollector;
        }

        public CachingFilterDecorator(IDictionary<string, CacheConfiguration> cacheConfiguration)
        {
            this.cacheConfiguration = cacheConfiguration;
        }

        public Task<MessageExchange> Process(IMessageExchangeFilter filter, MessageExchange exchange, MessageExchangeFilterInvoker invoker)
        {
            if (!exchange.IsIncompleteRequest)
            {
                return filter.Process(exchange, invoker);
            }

            var messageLabel = exchange.Out.Label.Name;

            CacheConfiguration config;
            if (!this.cacheConfiguration.TryGetValue(messageLabel, out config))
            {
                this.CollectMetrics(messageLabel, false);
                return filter.Process(exchange, invoker);
            }

            if (!(config.Enabled ?? false))
            {
                this.CollectMetrics(messageLabel, false);
                return filter.Process(exchange, invoker);
            }

            var cached = config.Cache.ContainsKey(exchange.Out);

            this.CollectMetrics(messageLabel, cached);

            if (cached)
            {
                exchange.In = new Message(MessageLabel.Empty, config.Cache[exchange.Out]);
                return Filter.Result(exchange);
            }

            return filter.Process(exchange, invoker)
                .ContinueWith(t => TryCacheResponse(t.Result, config));
        }

        private static MessageExchange TryCacheResponse(MessageExchange processedExchange, CacheConfiguration config)
        {
            if (!processedExchange.IsCompleteRequest)
            {
                return processedExchange;
            }

            var ttl = GetTtl(config, processedExchange);

            if (ttl != null)
            {
                config.Cache.Set(processedExchange.Out, processedExchange.In.Payload, ttl.Value);
            }

            return processedExchange;
        }

        private static TimeSpan? GetTtl(CacheConfiguration config, MessageExchange exchange)
        {
            if (config.Ttl != null)
            {
                return config.Ttl.Value;
            }

            var expiresHeader = Headers.GetString(exchange.In.Headers, Headers.Expires);
            if (!string.IsNullOrEmpty(expiresHeader))
            {
                var expiration = Expires.Parse(expiresHeader);
                if (expiration != null)
                {
                    if (expiration.Period != null)
                    {
                        return expiration.Period;
                    }

                    var now = DateTimeOffset.Now;
                    if (expiration.Date != null && expiration.Date.Value > now)
                    {
                        return expiration.Date.Value - now;
                    }
                }
            }

            return null;
        }

        private void CollectMetrics(string label, bool hit) => this.metricsCollector?.Increment("contour.outgoing.cache_usage.count", 1d, new[] { "cache:" + (hit ? "hit" : "miss"), "label:" + label });
    }
}