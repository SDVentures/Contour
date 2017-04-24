// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageExchangeFilter.cs" company="">
//   
// </copyright>
// <summary>
//   The MessageExchangeFilter interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Filters
{
    using System.Threading.Tasks;

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

        #endregion
    }
}
