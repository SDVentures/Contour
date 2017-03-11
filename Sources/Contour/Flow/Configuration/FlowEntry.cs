using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Represents an entry point for a flow
    /// </summary>
    public class FlowEntry<TInput> : IFlowEntry<TInput>
    {
        private readonly ITargetBlock<FlowContext<TInput>> target;

        /// <inheritdoc />
        public Guid Id { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FlowEntry{TInput}"/>
        /// </summary>
        public FlowEntry(ITargetBlock<FlowContext<TInput>> target)
        {
            this.Id = Guid.NewGuid();
            this.target = target;
        }
        
        /// <inheritdoc />
        public bool Post(TInput message)
        {
            var context = new FlowContext<TInput>() {Id = this.Id, In = message};
            return target.Post(context);
        }

        /// <inheritdoc />
        public Task<bool> PostAsync(TInput message)
        {
            var context = new FlowContext<TInput>() { Id = this.Id, In = message };
            return target.SendAsync(context);
        }

        /// <inheritdoc />
        public Task<bool> PostAsync(TInput message, CancellationToken token)
        {
            var context = new FlowContext<TInput>() { Id = this.Id, In = message };
            return target.SendAsync(context, token);
        }

        /// <inheritdoc />
        public ITargetBlock<FlowContext<TInput>> AsBlock()
        {
            return target;
        }
    }
}