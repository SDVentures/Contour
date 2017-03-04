using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a flow entry point
    /// </summary>
    public interface IFlowEntry<in TInput>
    {
        /// <summary>
        /// Entry point unique identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Provides an entry point as a flow block
        /// </summary>
        /// <returns></returns>
        ITargetBlock<TInput> AsBlock();

        /// <summary>
        /// Synchronously posts a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Post(TInput message);
        
        /// <summary>
        /// Asynchronously posts a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> PostAsync(TInput message);

        /// <summary>
        /// Asynchronously sends a message of <typeparamref name="TInput"/> type to the flow and provides a <paramref name="token"/> cancellation token to terminate the operation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> PostAsync(TInput message, CancellationToken token);
    }
}