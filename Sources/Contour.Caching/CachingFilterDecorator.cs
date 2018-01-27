namespace Contour.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Contour.Filters;

    public class CachingFilterDecorator : IMessageExchangeFilterDecorator
    {
        private readonly IDictionary<string, CacheConfiguration> cacheConfiguration;

        public CachingFilterDecorator(IDictionary<string, CacheConfiguration> cacheConfiguration)
        {
            this.cacheConfiguration = cacheConfiguration;
        }

        public Task<MessageExchange> Process(IMessageExchangeFilter filter, MessageExchange exchange, MessageExchangeFilterInvoker invoker)
        {
            if (!exchange.IsIncompleteRequest)
            {
                return invoker.Continue(exchange);
            }

            var messageLabel = exchange.Out.Label.Name;

            CacheConfiguration config;
            if (!this.cacheConfiguration.TryGetValue(messageLabel, out config))
            {
                return filter.Process(exchange, invoker);
            }

            if (!(config.Enabled ?? false) || config.Ttl == null)
            {
                return filter.Process(exchange, invoker);
            }

            if (config.Cache.ContainsKey(exchange.Out))
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
                return expiration?.Period;
            }

            return null;
        }
    }
}