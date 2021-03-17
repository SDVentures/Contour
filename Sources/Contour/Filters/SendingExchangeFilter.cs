namespace Contour.Filters
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The sending exchange filter.
    /// </summary>
    public class SendingExchangeFilter : IMessageExchangeFilter
    {
        #region Fields

        /// <summary>
        /// The _sending action.
        /// </summary>
        private readonly Func<MessageExchange, string, Task> _sendingAction;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SendingExchangeFilter"/>.
        /// </summary>
        /// <param name="sendingAction">
        /// The sending action.
        /// </param>
        public SendingExchangeFilter(Func<MessageExchange, string, Task> sendingAction)
        {
            this._sendingAction = sendingAction;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="invoker">
        /// The invoker.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker)
        {
            return this.Process(exchange, invoker, null);
        }

        public Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker, string url = null)
        {
            return this._sendingAction(exchange, url).ContinueWith(_ => invoker.Continue(exchange).Result);
        }
        #endregion
    }
}
