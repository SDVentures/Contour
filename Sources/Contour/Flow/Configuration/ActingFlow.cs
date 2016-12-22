using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    public class ActingFlow<TInput> : IActingFlow<TInput>
    {
        private readonly ISourceBlock<TInput> sourceBlock;

        public Task Completion => sourceBlock.Completion;

        public ActingFlow(ISourceBlock<TInput> sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        public IActingFlow<TOutput> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1)
        {
            var actBlock = new TransformBlock<TInput, TOutput>(act,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            sourceBlock.LinkTo(actBlock);
            var actFlow = new ActingFlow<TOutput>(actBlock);
            return actFlow;
        }

        public IOutgoingFlow<TOutput> Respond<TOutput>()
        {
            var destinationBlock = new DestinationMessageBlock<TOutput>();
            
            // todo add type validation here as the cast below is only possible after Act is called
            var propagatorBlock = (IPropagatorBlock<TInput, TOutput>) sourceBlock;
            propagatorBlock.LinkTo(destinationBlock);

            var outgoingFlow = new OutgoingFlow<TOutput>(destinationBlock);
            return outgoingFlow;
        }

        public ICachingFlow<TOutput> Cache<TOutput>(TimeSpan duration)
        {
            var cachingBlock = new CachingBlock<TInput, TOutput>(duration);
            sourceBlock.LinkTo(cachingBlock);

            var cachebleFlow = new CachingFlow<TOutput>(cachingBlock);
            return cachebleFlow;
        }

        public IOutgoingFlow<TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}