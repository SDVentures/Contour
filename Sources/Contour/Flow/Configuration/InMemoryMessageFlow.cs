using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides an in-memory flow implementation
    /// </summary>
    public class InMemoryMessageFlow : IMessageFlow
    {
        private readonly ILog log = LogManager.GetLogger<InMemoryMessageFlow>();
        private IDataflowBlock block;

        /// <summary>
        /// Flow label
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// A flow registry which provides flow coordination in request-response and broadcasting scenarios.
        /// </summary>
        public IFlowRegistry Registry { private get; set; }

        /// <summary>
        /// Registers a new flow of <typeparamref name="TOutput"/> typed items.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="label">Flow label</param>
        /// <param name="capacity">Specifies the maximum capacity of the flow pipeline</param>
        /// <returns></returns>
        public IActingFlow<TOutput> On<TOutput>(string label, int capacity = 1)
        {
            if (block != null)
                throw new FlowConfigurationException($"Flow [{Label}] has already been configured");

            Label = label;
            block = new BufferBlock<TOutput>(new DataflowBlockOptions() {BoundedCapacity = capacity});
            var flow = new ActingFlow<TOutput>((ISourceBlock<TOutput>) block) {Registry = Registry};
            return flow;
        }
        
        bool IFlowEntry.Post<TInput>(TInput message)
        {
            EnsureSourceConfigured();

            var target = (ITargetBlock<TInput>) block;
            return target.Post(message);
        }
        
        Task<bool> IFlowEntry.SendAsync<TInput>(TInput message)
        {
            EnsureSourceConfigured();

            var target = (ITargetBlock<TInput>)block;
            return target.SendAsync(message);
        }

        Task<bool> IFlowEntry.SendAsync<TInput>(TInput message, CancellationToken token)
        {
            EnsureSourceConfigured();

            var target = (ITargetBlock<TInput>)block;
            return target.SendAsync(message, token);
        }

        ITargetBlock<TOutput> IFlowTarget.AsTarget<TOutput>()
        {
            EnsureSourceConfigured();
            
            return block as ITargetBlock<TOutput>;
        }

        private void EnsureSourceConfigured()
        {
            if (block == null)
            {
                throw new FlowConfigurationException($"Flow [{Label}] is not yet configured, call On<> method first.");
            }
        }
    }
}