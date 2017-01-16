using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    internal class OutgoingFlow<TInput>: IOutgoingFlow<TInput>
    {
        private readonly ITargetBlock<TInput> targetBlock;

        public OutgoingFlow(ITargetBlock<TInput> targetBlock)
        {
            this.targetBlock = targetBlock;
        }
    }
}