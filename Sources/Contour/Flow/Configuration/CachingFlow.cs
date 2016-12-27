using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class CachingFlow<TOutput> : ICachingFlow<TOutput>
    {
        private IDataflowBlock sourceBlock;

        public CachingFlow(IDataflowBlock sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        public IOutgoingFlow<TOutput> Respond()
        {
            throw new NotImplementedException();
        }

        public IOutgoingFlow<TOutput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}