using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    public class ActingFlow<TInput> : IActingFlow<TInput>
    {
        private readonly ISourceBlock<TInput> sourceBlock;

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

        public IOutgoingFlow<TInput> Respond()
        {
            var destinationBlock = new DestinationMessageBlock<TInput>();
            sourceBlock.LinkTo(destinationBlock);

            var outgoingFlow = new OutgoingFlow<TInput>(destinationBlock);
            return outgoingFlow;
        }

        public ICachingFlow<TInput> Cache(TimeSpan duration)
        {
            var cachingBlock = new CachingBlock<TInput>(duration);
            sourceBlock.LinkTo(cachingBlock);

            var cachingFlow = new CachingFlow<TInput>(cachingBlock);
            return cachingFlow;
        }

        public IOutgoingFlow<TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}