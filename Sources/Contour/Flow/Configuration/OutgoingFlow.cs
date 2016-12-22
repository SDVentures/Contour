using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class OutgoingFlow<TInput>: IOutgoingFlow<TInput>
    {
        private readonly ITargetBlock<TInput> targetBlock;

        public OutgoingFlow(ITargetBlock<TInput> dataflowBlock)
        {
            throw new NotImplementedException();
        }

        public Task Completion { get; }
    }
}