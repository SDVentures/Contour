namespace Contour.Filters
{
    using System;
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

        private readonly IDictionary<Type, IMessageExchangeFilterDecorator> filterDecorators;


        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageExchangeFilterInvoker"/>.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        public MessageExchangeFilterInvoker(IEnumerable<IMessageExchangeFilter> filters)
            : this(filters, null)
        {
        }

        public MessageExchangeFilterInvoker(IEnumerable<IMessageExchangeFilter> filters, IDictionary<Type, IMessageExchangeFilterDecorator> filterDecorators)
        {
            this.filterEnumerator = filters.Reverse().
                GetEnumerator();
            this.filterDecorators = filterDecorators ?? new Dictionary<Type, IMessageExchangeFilterDecorator>();
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

            var currentFilter = this.filterEnumerator.Current;
            var currentFilterType = currentFilter.GetType();

            if (this.filterDecorators.ContainsKey(currentFilterType))
            {
                return this.filterDecorators[currentFilterType].Process(currentFilter, exchange, this);
            }

            return currentFilter.Process(exchange, this);
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
