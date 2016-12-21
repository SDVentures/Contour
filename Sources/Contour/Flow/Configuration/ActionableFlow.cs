using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class ActionableFlow<TInput> : IActionableFlow<TInput>
    {
        private readonly ISourceBlock<TInput> sourceBlock;

        public ActionableFlow(ISourceBlock<TInput> sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        public IActionableFlow<TOutput> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1)
        {
            var actBlock = new TransformBlock<TInput, TOutput>(act,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            sourceBlock.LinkTo(actBlock);
            var actSourceBlock = (ISourceBlock<TOutput>) actBlock;
            var actFlow = new ActionableFlow<TOutput>(actSourceBlock);
            return actFlow;
        }

        public Task Completion => sourceBlock.Completion;
        

        public ICachebleFlow Respond<TOutput>(Func<TOutput> responder) //todo should support configuration here?
        {
            throw new NotImplementedException();
        }

        public ICachebleFlow Respond() //todo should support configuration here?
        {
            var bufferBlock = new BufferBlock<TInput>();
            sourceBlock.LinkTo(bufferBlock);

            var cachebleFlow = new CachebleFlow<TInput>(bufferBlock);
            return cachebleFlow;
        }

        public IMessageFlow Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}