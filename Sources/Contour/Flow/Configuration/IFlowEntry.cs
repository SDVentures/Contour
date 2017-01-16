using System.Threading;
using System.Threading.Tasks;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a flow entry point
    /// </summary>
    public interface IFlowEntry
    {
        /// <summary>
        /// Synchronously posts a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        bool Post<TInput>(TInput message);

        /// <summary>
        /// Asynchronously sends a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        Task<bool> SendAsync<TInput>(TInput message);

        /// <summary>
        /// Asynchronously sends a message of <typeparamref name="TInput"/> type to the flow and provides a <paramref name="token"/> cancellation token to terminate the operation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        Task<bool> SendAsync<TInput>(TInput message, CancellationToken token);
    }
}