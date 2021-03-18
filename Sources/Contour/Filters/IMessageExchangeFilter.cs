using System.Threading.Tasks;

namespace Contour.Filters
{
    /// <summary>
    /// The MessageExchangeFilter interface.
    /// </summary>
    public interface IMessageExchangeFilter
    {
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
        Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker);


        Task<MessageExchange> Process(MessageExchange exchange, MessageExchangeFilterInvoker invoker, string connectionKey);

        #endregion
    }
}
