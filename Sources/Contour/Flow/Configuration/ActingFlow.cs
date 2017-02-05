using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Caching;
using Contour.Flow.Blocks;
using Contour.Flow.Execution;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class ActingFlow<TInput> : IActingFlow<TInput>, IActingFlowConcatenation<TInput>
    {
        private readonly ILog log = LogManager.GetLogger<ActingFlow<TInput>>();

        private readonly ISourceBlock<TInput> source;
        private readonly IDataflowBlock tail;
        private readonly IDisposable tailLink;

        public IFlowRegistry Registry { private get; set; }
        public IFlowTransport Transport { private get; set; }

        public ActingFlow(ISourceBlock<TInput> source, IDataflowBlock tail = null, IDisposable tailLink = null)
        {
            this.source = source;
            this.tail = tail;
            this.tailLink = tailLink;
        }

        public IActingFlow<FlowContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1)
        {
            Func<TInput, FlowContext<TInput, TOutput>> func = input =>
            {
                var output = default(TOutput);
                Exception exception = null;
                
                try
                {
                    log.Debug(new { name = nameof(Act), act, input, capacity, scale });
                    output = act(input);
                }
                catch (Exception ex)
                {
                    log.Error(new {name = nameof(Act), error = ex, input, capacity, scale});
                    exception = ex;
                }

                return new FlowContext<TInput, TOutput> {In = input, Out = output, Error = exception};
            };

            //Need to stop the flow in case of any errors in the action
            Predicate<FlowContext<TInput, TOutput>> onSuccess = context => context.Error == null;
            
            var action = new ConditionalTransformBlock<TInput, FlowContext<TInput, TOutput>>(func, onSuccess,
                new ExecutionDataflowBlockOptions {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});
            var link = source.LinkTo(action);

            var flow = new ActingFlow<FlowContext<TInput, TOutput>>(action, source, link)
            {
                Registry = this.Registry,
                Transport = this.Transport
            };
            return flow;
        }

        public ITerminatingFlow Act(Action<TInput> act, int capacity = 1, int scale = 1)
        {
            Func<TInput, Task<FlowContext<TInput>>> func = input =>
            {
                Exception exception = null;

                try
                {
                    log.Debug(new { name = nameof(Act), act, input, capacity, scale });
                    act(input);
                }
                catch (Exception ex)
                {
                    log.Error(new {name = nameof(Act), error = ex, input, capacity, scale});
                    exception = ex;
                }

                return Task.FromResult(new FlowContext<TInput> {In = input, Error = exception});
            };
            
            var action = new ActionBlock<TInput>(func,
                new ExecutionDataflowBlockOptions() {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});

            source.LinkTo(action);
            var flow = new TerminatingFlow();
            return flow;
        }

        public IOutgoingFlow<TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class
        {
            //The pipe tail should be a source for the outgoing flow
            var tailAsSource = (ISourceBlock<TIn>)tail;
            
            //Detach from original source
            tailLink.Dispose();

            //Attach tail to cache; to do that the source items need to be converted to tuples with empty results
            var cacheInFunc =
                new Func<TIn, FlowContext<TIn, TOut>>(
                    @in => new FlowContext<TIn, TOut> {In = @in, Out = default(TOut)});

            var cacheInTransform = new TransformBlock<TIn, FlowContext<TIn, TOut>>(cacheInFunc);

            tailAsSource.LinkTo(cacheInTransform);

            //WARN: do not limit the caching block capacity to 1 as it can block the flow indefinitely
            var cache = new CachingBlock<TIn, TOut>(policy);

            var cacheAsTarget = (ITargetBlock<FlowContext<TIn, TOut>>)cache;
            cacheInTransform.LinkTo(cacheAsTarget);
            
            //Attach cache to the source to adapt cache output to the source input
            var cacheOutFunc = new Func<FlowContext<TIn, TOut>, TIn>(ctx => ctx.In);
            var cacheOutTransform = new TransformBlock<FlowContext<TIn, TOut>, TIn>(cacheOutFunc);
            cache.MissLinkTo(cacheOutTransform, new DataflowLinkOptions());

            //Attach cache as source via cache out transform
            //The source should be convertible to a target to accept flow items not found in cache
            var sourceAsTarget = (ITargetBlock<TIn>)source;
            cacheOutTransform.LinkTo(sourceAsTarget);

            //Direct source block execution results to the cache
            ((ISourceBlock<FlowContext<TIn,TOut>>)source).LinkTo(cacheAsTarget);

            //Pass cache block as source for outgoing flow
            var outgoingFlow = new OutgoingFlow<TOut>(cache);
            return outgoingFlow;
        }

        public IActingFlowConcatenation<FlowContext<TIn, TOut>> Broadcast<TIn, TOut>(string label = null, int capacity = 1, int scale = 1)
        {
            //Broadcasting for action results: this flow's source block (it is transform block in fact) needs to be attached to the broadcast block, which in turn will be attached to all the flows provided by the registry

            //The action block should return a tuple of input and output to support results caching, so the source items need to be converted to the action return type

            var broadcast = new BroadcastBlock<TOut>(p => p, new DataflowBlockOptions() { BoundedCapacity = capacity });
            var actionOutTransform = new TransformBlock<FlowContext<TIn, TOut>, TOut>(t => t.Out, new ExecutionDataflowBlockOptions() {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});

            //todo: check the source type before
            ((ISourceBlock<FlowContext<TIn, TOut>>)source).LinkTo(actionOutTransform);
            actionOutTransform.LinkTo(broadcast);

            //Get all flows by specific type from the registry (flow label is irrelevant here due to possible flow items type casting errors)
            var flows = Registry.GetAll<TOut>();
            foreach (var flow in flows)
            {
                if (flow is IMessageFlow<TOut>)
                {
                    var messageFlow = flow as IMessageFlow<TOut>;
                    broadcast.LinkTo(messageFlow.AsTarget());
                }
            }

            return (IActingFlowConcatenation<FlowContext<TIn, TOut>>) this;
        }

        public IResponseFlow<TInput> Respond(int capacity = 1)
        {
            //Create a tail flow to broadcast flow results; only clients with specific correlation queries will get the results

            var broadcast = new BroadcastBlock<TInput>(p => p, new DataflowBlockOptions() {BoundedCapacity = capacity});
            source.LinkTo(broadcast);

            var flow = new ResponseFlow<TInput>(broadcast);
            return flow;
        }

        public IResponseFlow<TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}