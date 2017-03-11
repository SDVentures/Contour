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
    internal class ActingFlow<TSource, TInput> : IActingFlow<TSource, TInput>, IActingFlowConcatenation<TSource, TInput>
    {
        private readonly ILog log = LogManager.GetLogger<ActingFlow<TSource, TInput>>();

        private readonly ISourceBlock<TInput> source;

        public IFlowRegistry Registry { private get; set; }
        public string Label { private get; set; }

        public ActingFlow(ISourceBlock<TInput> source)
        {
            this.source = source;
        }

        public IActingFlow<TSource, FlowContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput: class
        {
            var actionWrapper = GetCachingActionWrapper(act, capacity, scale, policy);

            //Need to stop the flow in case of any errors in the action
            Predicate<FlowContext<TInput, TOutput>> successPredicate = context => context.Error == null;

            var action = new ConditionalTransformBlock<TInput, FlowContext<TInput, TOutput>>(actionWrapper, successPredicate,
                new ExecutionDataflowBlockOptions { BoundedCapacity = capacity, MaxDegreeOfParallelism = scale });
            source.LinkTo(action);

            var flow = new ActingFlow<TSource, FlowContext<TInput, TOutput>>(action)
            {
                Registry = this.Registry,
                Label = this.Label
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
        
        public IActingFlowConcatenation<TSource, TInput> Broadcast<TOutput>(Func<TInput, TOutput> act, string label = null, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput : class
        {

            var actionWrapper = GetCachingActionWrapper(act, capacity, scale, policy);

            //Need to stop the flow in case of any errors in the action
            Predicate<FlowContext<TInput, TOutput>> successPredicate = context => context.Error == null;

            var action = new ConditionalTransformBlock<TInput, FlowContext<TInput, TOutput>>(actionWrapper,
                successPredicate,
                new ExecutionDataflowBlockOptions {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});

            var transform =
                new TransformBlock<FlowContext<TInput, TOutput>, FlowContext<TOutput>>(
                    context => new FlowContext<TOutput>() {Error = context.Error, In = context.Out, Id = context.Id});
            
            source.LinkTo(action);
            action.LinkTo(transform);

            // todo: provide a deep copying cloning function for broadcast
            var broadcast = new BroadcastBlock<FlowContext<TOutput>>(p => p,
                new DataflowBlockOptions() { BoundedCapacity = capacity });

            // Broadcast the action results
            transform.LinkTo(broadcast);

            // Get all flows by specific type from the registry (flow label is irrelevant here due to possible flow items type casting errors)
            //todo: get flows by label if provided
            var flows = Registry.GetAll();
            foreach (var flow in flows)
            {
                var head = flow as IMessageFlow<TOutput, FlowContext<TOutput>>;
                if (head != null)
                {
                    broadcast.LinkTo(head.Entry().AsBlock());
                }
            }

            return this;
        }

        public IRequestResponseFlow<TSource, TInput> Respond()
        {
            // Create a tail flow to broadcast flow results, only clients with specific correlation queries will get the results

            //todo: provide a deep copying cloning function for the broadcast block
            var broadcast = new BroadcastBlock<TInput>(p => p);
            source.LinkTo(broadcast);

            var flow = new RequestResponseFlow<TSource, TInput>(broadcast)
            {
                Registry = this.Registry,
                Label = this.Label
            };
            return flow;
        }

        public IRequestResponseFlow<TSource, TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }


        private Func<TInput, FlowContext<TInput, TOutput>> GetCachingActionWrapper<TOutput>(Func<TInput, TOutput> act, int capacity, int scale, ICachePolicy policy) where TOutput : class
        {
            return input =>
            {
                var output = default(TOutput);
                Exception exception = null;

                try
                {
                    log.Debug($"Executing [{act}] on [{input}] at scale={scale}, capacity={capacity}");

                    if (policy != null)
                    {
                        log.Debug($"Using [{policy}] as cache policy");

                        var key = policy.KeyProvider.Get(input);
                        log.Debug($"A key [{key}] for [{input}] argument acquired");

                        try
                        {
                            output = policy.CacheProvider.Get<TOutput>(key);
                            log.Debug(output != null ? $"Cache hit: key=[{key}]" : $"Cache miss: key=[{key}]");
                        }
                        catch (Exception ex)
                        {
                            log.Error($"Failed to get a cached output for action [{act}] due to {ex.Message}");
                            exception = ex;
                        }

                        if (output == null)
                        {
                            log.Trace($"Executing the action [{act}]");
                            output = act(input);
                            log.Trace($"Action [{act}] executed successfully, caching the result");

                            policy.CacheProvider.Put(key, output, policy.Period);
                            log.Trace(
                                $"Action [{act}] execution result [{output}] has been cached with [{policy}] for [{policy.Period}]");
                        }
                    }
                    else
                    {
                        log.Trace($"No caching policy is provided, executing the action [{act}]");
                        output = act(input);
                        log.Trace($"Action [{act}] executed successfully");
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Action [{act}({input}, capacity:{capacity}, scale:{scale})] execution failed due to {ex.Message}");
                    exception = ex;
                }

                return new FlowContext<TInput, TOutput> { In = input, Out = output, Error = exception };
            };
        }
    }
}