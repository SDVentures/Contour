using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class InMemoryMessageFlow : IMessageFlow
    {
        private IDataflowBlock block;

        public string Id { get; } = Guid.NewGuid().ToString();

        public IActingFlow<TOutput> On<TOutput>(string label, int capacity = 1)
        {
            if (block != null)
                throw new FlowConfigurationException($"Flow [{Id}] has already been configured");

            block = new BufferBlock<TOutput>(new DataflowBlockOptions() {BoundedCapacity = capacity});
            var flow = new ActingFlow<TOutput>((ISourceBlock<TOutput>) block);
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
        
        private void EnsureSourceConfigured()
        {
            if (block == null)
            {
                throw new FlowConfigurationException($"Flow [{Id}] is not yet configured, call On<> method first.");
            }
        }
    }
}