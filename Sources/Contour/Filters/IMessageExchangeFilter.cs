namespace Contour.Filters
{
    using System.Threading.Tasks;

    /// <summary>
    /// The MessageExchangeFilter interface.
    /// </summary>
    public interface IMessageExchangeFilter
    {
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
    }
}
