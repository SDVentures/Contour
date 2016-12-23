using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class CachingFlow<TInput> : ICachingFlow<TInput>
    {
        private readonly ISourceBlock<TInput> sourceBlock;

        public CachingFlow(ISourceBlock<TInput> sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        public IOutgoingFlow<TInput> Respond()
        {
            throw new NotImplementedException();
        }
    }
}