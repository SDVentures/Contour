using System;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Caching;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    internal class ActingFlow<TInput> : IActingFlow<TInput>, IActingFlowConcatenation<TInput>
    {
        private readonly ILog log = LogManager.GetLogger<ActingFlow<TInput>>();

        private readonly ISourceBlock<TInput> source;
        private readonly IDataflowBlock tail;
        private readonly IDisposable tailLink;
        
        public IFlowRegistry Registry { private get; set; }

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
                log.Debug(new {name = nameof(Act), act, input, capacity, scale});
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
            var flow = new ActingFlow<Tuple<TInput, TOutput>>(transform, source, link) {Registry = Registry};
            return flow;
        }
        
        public IOutgoingFlow Cache<TIn, TOut>(ICachePolicy policy) where TOut : class
        {
            //The pipe tail should be a source for the outgoing flow
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
            
            //Attach cache to the source to adapt cache output to the source input
            var cacheOutFunc = new Func<Tuple<TIn, TOut>, TIn>(tuple => tuple.Item1);
            var cacheOutTransform = new TransformBlock<Tuple<TIn, TOut>, TIn>(cacheOutFunc);
            cache.MissLinkTo(cacheOutTransform, new DataflowLinkOptions());

            //Attach cache as source via cache out transform
            //The source should be convertible to a target to accept flow items not found in cache
            var sourceAsTarget = (ITargetBlock<TIn>)source;
            cacheOutTransform.LinkTo(sourceAsTarget);

            //Direct source block execution results to the cache
            ((ISourceBlock<Tuple<TIn,TOut>>)source).LinkTo(cacheAsTarget);

            //Pass cache block as source for outgoing flow
            var outgoingFlow = new OutgoingFlow(cache);
            return outgoingFlow;
        }

        public IActingFlowConcatenation<Tuple<TIn, TOut>> Broadcast<TIn, TOut>(string label = null, int capacity = 1, int scale = 1)
        {
            //Broadcasting is only possible for action results, i.e. this flow's source block (it is transform block in fact) needs to be attached to the broadcast block, which in turn will be attached to all the flows provided by the registry

            //The action block should return a tuple of input and output to support results caching, so the source items need to be converted to the action return type
            var broadcast = new BroadcastBlock<TOut>(p => p, new DataflowBlockOptions() {BoundedCapacity = capacity});
            var actionOutTransform = new TransformBlock<Tuple<TIn, TOut>, TOut>(t => t.Item2, new ExecutionDataflowBlockOptions() {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});

            ((ISourceBlock<Tuple<TIn, TOut>>)source).LinkTo(actionOutTransform);
            actionOutTransform.LinkTo(broadcast);

            //Get all flows by specific type from the registry (flow label is irrelevant here due to possible flow items type casting errors)
            var flows = Registry.Get<TOut>();
            foreach (var flow in flows)
            {
                broadcast.LinkTo(flow.AsTarget<TOut>());
            }

            return (IActingFlowConcatenation<Tuple<TIn, TOut>>) this;
        }

        public void Respond()
        {
            throw new NotImplementedException();
        }

        public void Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}