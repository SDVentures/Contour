using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    public class ActingFlow<TInput> : IActingFlow<TInput>
    {
        private readonly ISourceBlock<TInput> source;
        private IDataflowBlock tail;
        private IDisposable tailLink;
        
        public ActingFlow(ISourceBlock<TInput> source, IDataflowBlock tail = null, IDisposable tailLink = null)
        {
            this.source = source;
            this.tail = tail;
            this.tailLink = tailLink;
        }

        public IActingFlow<Tuple<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1)
        {
            Func<TInput, Tuple<TInput, TOutput>> func = input =>
            {
                var output = act(input);
                return new Tuple<TInput, TOutput>(input, output);
            };

            var transform = new TransformBlock<TInput, Tuple<TInput, TOutput>>(func,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            var link = source.LinkTo(transform);
            var flow = new ActingFlow<Tuple<TInput, TOutput>>(transform, source, link);
            return flow;
        }

        public ICachingFlow<TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class
        {
            //The pipe tail should be a source for the caching flow
            var tailAsSource = (ISourceBlock<TIn>)tail;
            
            //Detach from original source
            tailLink.Dispose();

            //Attach tail to cache; to do that the source items need to be converted to tuples with empty results
            var cacheInFunc = new Func<TIn, Tuple<TIn, TOut>>(@in => new Tuple<TIn, TOut>(@in, default(TOut)));
            var cacheInTransform = new TransformBlock<TIn, Tuple<TIn, TOut>>(cacheInFunc);

            tailAsSource.LinkTo(cacheInTransform);
            var cache = new CachingBlock<TIn, TOut>(policy);
            var cacheAsTarget = (ITargetBlock<Tuple<TIn, TOut>>)cache;
            cacheInTransform.LinkTo(cacheAsTarget);
            
            //Attach cache to the source to convert cached items back to the source input type
            var cacheOutFunc = new Func<Tuple<TIn, TOut>, TIn>(tuple => tuple.Item1);
            var cacheOutTransform = new TransformBlock<Tuple<TIn, TOut>, TIn>(cacheOutFunc);

            cache.LinkMissed(cacheOutTransform, new DataflowLinkOptions());

            //The source should be convertible to a target to accept the flow items not found in cache
            //Attach cache as the source
            var sourceAsTarget = (ITargetBlock<TIn>)source;
            cacheOutTransform.LinkTo(sourceAsTarget);

            //Direct the results back to the cache
            ((ISourceBlock<Tuple<TIn,TOut>>)source).LinkTo(cacheAsTarget);

            //Attach the cache block as the source for this block
            var cachingFlow = new CachingFlow<TOut>(cache);
            return cachingFlow;
        }
        
        public IOutgoingFlow<TInput> Respond()
        {
            var destination = new DestinationMessageBlock<TInput>();
            source.LinkTo(destination);

            var outgoingFlow = new OutgoingFlow<TInput>(destination);
            return outgoingFlow;
        }
        
        public IOutgoingFlow<TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}