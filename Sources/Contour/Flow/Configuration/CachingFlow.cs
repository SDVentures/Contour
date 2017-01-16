using System;
using System.Threading.Tasks.Dataflow;
using Contour.Caching;

namespace Contour.Flow.Configuration
{
    internal class CachingFlow<TOutput> : ICachingFlow<TOutput>
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

        public ICachingFlow<TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class
        {
            throw new NotImplementedException();
        }
    }
}