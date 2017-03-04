using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a flow entry point
    /// </summary>
    public interface IFlowEntry<TSource>
    {
        /// <summary>
        /// Entry point unique identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Provides an entry point as a flow block
        /// </summary>
        /// <returns></returns>
        ITargetBlock<FlowContext<TSource>> AsBlock();

        /// <summary>
        /// Synchronously posts a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Post(TSource message);
        
        /// <summary>
        /// Asynchronously posts a message of <typeparamref name="TInput"/> type to the flow
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> PostAsync(TSource message);

        /// <summary>
        /// Asynchronously sends a message of <typeparamref name="TInput"/> type to the flow and provides a <paramref name="token"/> cancellation token to terminate the operation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> PostAsync(TSource message, CancellationToken token);
    }
}