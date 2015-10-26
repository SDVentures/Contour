using System.Threading.Tasks;

using Contour.Filters;
using Contour.Helpers;

namespace Contour.Caching
{
    /// <summary>
    /// Фильтр, который кеширует ответные сообщения.
    /// </summary>
    public class CacheMessageExchangeFilter : IMessageExchangeFilter
    {
        /// <summary>
        /// Поставщик кеша.
        /// </summary>
        private readonly ICacheProvider cacheProvider;

        /// <summary>
        /// Поставщик кеш значения.
        /// </summary>
        private readonly Hasher hasher = new Hasher();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CacheMessageExchangeFilter"/>.
        /// </summary>
        /// <param name="cacheProvider">Поставщик кеша.</param>
        public CacheMessageExchangeFilter(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Если в кеше, есть ответное сообщение на входящее сообщение, тогда возвращает объект из кеша.
        /// </summary>
        /// <param name="exchange">Конвейер обработки сообщений.</param>
        /// <param name="invoker">Фильтры вызывающий конвейер.</param>
        /// <returns>Задача обработки сообщений.</returns>
        public Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker)
        {
            if (!exchange.IsIncompleteRequest)
            {
                return invoker.Continue(exchange);
            }

            string hash = this.hasher.CalculateHashOf(exchange.Out).ToString();
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
