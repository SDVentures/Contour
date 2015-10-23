// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Filter.cs" company="">
//   
// </copyright>
// <summary>
//   The filter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Filters
{
    using System.Threading.Tasks;

    /// <summary>
    /// The filter.
    /// </summary>
    public static class Filter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The result.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<MessageExchange> Result(MessageExchange exchange)
        {
            var completionSource = new TaskCompletionSource<MessageExchange>();
            completionSource.SetResult(exchange);
            return completionSource.Task;
        }

        #endregion
    }
}
