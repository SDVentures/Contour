namespace Contour.Filters
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The sending exchange filter.
    /// </summary>
    public class SendingExchangeFilter : IMessageExchangeFilter
    {
        /// <summary>
        /// The _sending action.
        /// </summary>
        private readonly Func<MessageExchange, Task> _sendingAction;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SendingExchangeFilter"/>.
        /// </summary>
        /// <param name="sendingAction">
        /// The sending action.
        /// </param>
        public SendingExchangeFilter(Func<MessageExchange, Task> sendingAction)
        {
            this._sendingAction = sendingAction;
        }
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
            return this._sendingAction(exchange).ContinueWith(_ => invoker.Continue(exchange).Result);
        }
    }
}
