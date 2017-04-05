namespace Contour.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The message exchange filter invoker.
    /// </summary>
    public class MessageExchangeFilterInvoker
    {
        /// <summary>
        /// The _filter enumerator.
        /// </summary>
        private readonly IEnumerator<IMessageExchangeFilter> filterEnumerator;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageExchangeFilterInvoker"/>.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        public MessageExchangeFilterInvoker(IEnumerable<IMessageExchangeFilter> filters)
        {
            this.filterEnumerator = filters.Reverse().
                GetEnumerator();
        }
        /// <summary>
        /// Gets or sets the inner.
        /// </summary>
        public IMessageExchangeFilter Inner { get; set; }
        /// <summary>
        /// The continue.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task<MessageExchange> Continue(MessageExchange exchange)
        {
            if (!this.filterEnumerator.MoveNext())
            {
                return Filter.Result(exchange);
            }

            return this.filterEnumerator.Current.Process(exchange, this);
        }

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public virtual Task<MessageExchange> Process(MessageExchange exchange)
        {
            return this.Continue(exchange);
        }
    }
}
