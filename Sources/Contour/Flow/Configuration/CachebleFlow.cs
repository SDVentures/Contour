using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class CachebleFlow<TInput> : ICachebleFlow
    {
        private readonly ISourceBlock<TInput> sourceBlock;

        public CachebleFlow(ISourceBlock<TInput> sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        public IMessageFlow CacheFor(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        public IMessageFlow NoCache()
        {
            throw new NotImplementedException();
        }

        public Task Completion => sourceBlock.Completion;
    }
}