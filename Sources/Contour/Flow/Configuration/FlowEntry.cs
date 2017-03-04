using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Represents an entry point for a flow
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class FlowEntry<TInput> : IFlowEntry<TInput>
    {
        private readonly ITargetBlock<TInput> target;

        /// <inheritdoc />
        public Guid Id { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FlowEntry{TInput}"/>
        /// </summary>
        public FlowEntry(ITargetBlock<TInput> target)
        {
            this.Id = Guid.NewGuid();
            this.target = target;
        }

        /// <inheritdoc />
        public ITargetBlock<TInput> AsBlock()
        {
            return target;
        }

        /// <inheritdoc />
        public bool Post(TInput message)
        {
            return target.Post(message);
        }

        /// <inheritdoc />
        public Task<bool> PostAsync(TInput message)
        {
            return target.SendAsync(message);
        }

        /// <inheritdoc />
        public Task<bool> PostAsync(TInput message, CancellationToken token)
        {
            return target.SendAsync(message, token);
        }
    }
}