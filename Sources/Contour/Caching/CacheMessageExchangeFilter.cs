using System.Threading.Tasks;

using Contour.Filters;
using Contour.Helpers;

namespace Contour.Caching
{
    /// <summary>
    /// Cache responses.
    /// </summary>
    public class CacheMessageExchangeFilter : IMessageExchangeFilter
    {
        private readonly ICacheProvider cacheProvider;

        private readonly IHashCalculator hashCalculator;

        /// <summary>
        /// Initialize new instance of <see cref="CacheMessageExchangeFilter"/>.
        /// </summary>
        /// <param name="cacheProvider">The cache storage provider to store responses.</param>
        /// <param name="hashCalculator">The hash calculator to get unique key for cache.</param>
        public CacheMessageExchangeFilter(ICacheProvider cacheProvider, IHashCalculator hashCalculator)
        {
            this.cacheProvider = cacheProvider;
            this.hashCalculator = hashCalculator;
        }

        /// <inheritdoc />
        public Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker)
        {
            if (!exchange.IsIncompleteRequest)
            {
                return invoker.Continue(exchange);
            }

            string hash = this.hashCalculator.CalculateHash(exchange.Out);
            Maybe<object> cached = this.cacheProvider.Find<object>(hash);
            if (cached.HasValue)
            {
                exchange.In = new Message(MessageLabel.Empty, cached.Value);
                return Filter.Result(exchange);
            }

            return invoker.Continue(exchange)
                .ContinueWith(
                    t =>
                        {
                            MessageExchange resultExchange = t.Result;
                            if (!resultExchange.IsCompleteRequest)
                            {
                                return resultExchange;
                            }

                            string expiresHeader = Headers.GetString(resultExchange.In.Headers, Headers.Expires);
                            if (!string.IsNullOrEmpty(expiresHeader))
                            {
                                Expires expiration = Expires.Parse(expiresHeader);

                                if (expiration.Period.HasValue)
                                {
                                    this.cacheProvider.Put(hash, resultExchange.In.Payload, expiration.Period.Value);
                                }
                                else
                                {
                                    this.cacheProvider.Put(hash, resultExchange.In.Payload, expiration.Date.Value);
                                }
                            }

                            return resultExchange;
                        });
        }
    }
}
