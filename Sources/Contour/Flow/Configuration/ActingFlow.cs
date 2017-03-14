using System;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Caching;
using Contour.Flow.Blocks;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    internal class ActingFlow<TSource, TInput> : IActingFlow<TSource, TInput>, IActingFlowConcatenation<TSource, TInput>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(TSource));

        private readonly ISourceBlock<FlowContext<TInput>> source;

        public IFlowRegistry Registry { private get; set; }
        public string Label { private get; set; }

        public ActingFlow(ISourceBlock<FlowContext<TInput>> source)
        {
            this.source = source;
        }

        public IActingFlow<TSource, TOutput> Act<TOutput>(Func<FlowContext<TInput>, TOutput> act, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput: class
        {
            var actionWrapper = GetCachingActionWrapper(act, capacity, scale, policy);
            
            Predicate<FlowContext<TOutput>> successPredicate = context => context.Error == null;

            var action = new ConditionalTransformBlock<FlowContext<TInput>, FlowContext<TOutput>>(
                actionWrapper,
                successPredicate,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            source.LinkTo(action);

            var flow = new ActingFlow<TSource, TOutput>(action)
            {
                Registry = this.Registry,
                Label = this.Label
            };
            return flow;
        }
        
        public ITerminatingFlow Act(Action<FlowContext<TInput>> act, int capacity = 1, int scale = 1)
        {
            Action<FlowContext<TInput>> actionWrapper = input =>
            {
                try
                {
                    log.Debug(new { name = nameof(Act), act, input, capacity, scale });
                    act(input);
                }
                catch (Exception ex)
                {
                    log.Error(new {name = nameof(Act), error = ex, input, capacity, scale});
                }
            };
            
            var action = new ActionBlock<FlowContext<TInput>>(actionWrapper,
                new ExecutionDataflowBlockOptions() {BoundedCapacity = capacity, MaxDegreeOfParallelism = scale});

            source.LinkTo(action);
            var flow = new TerminatingFlow();
            return flow;
        }
        
        public IActingFlowConcatenation<TSource, TOutput> Broadcast<TOutput>(Func<FlowContext<TInput>, TOutput> act, string label = null, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput : class
        {
            var actionWrapper = GetCachingActionWrapper(act, capacity, scale, policy);

            //Need to stop the flow in case of any errors in the action
            Predicate<FlowContext<TOutput>> successPredicate = context => context.Error == null;

            var actionBlock = new ConditionalTransformBlock<FlowContext<TInput>, FlowContext<TOutput>>(
                actionWrapper,
                successPredicate,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });
            
            source.LinkTo(actionBlock);
            log.Trace($"A source block [{source}] has been linked to action block [{actionBlock}]");

            // todo: provide a deep copying cloning function for broadcast
            var broadcastBlock = new BroadcastBlock<FlowContext<TOutput>>(p => p,
                new DataflowBlockOptions() { BoundedCapacity = capacity });

            // Broadcast the action results
            actionBlock.LinkTo(broadcastBlock);
            log.Trace($"Action block results will be broadcasted via [{broadcastBlock}]");

            // Get all flows by specific type from the registry (flow label is irrelevant here due to possible flow items type casting errors)
            //todo: get flows by label if provided

            log.Trace($"Searching for the flow accepting [{nameof(TOutput)}] as input...");
            var flows = Registry.GetAll<TOutput>();
            foreach (var flow in flows)
            {
                var head = flow as IMessageFlow<TOutput, TOutput>;
                if (head != null)
                {
                    log.Trace($"Linking broadcast block to [{head}]");
                    broadcastBlock.LinkTo(head.Entry().AsBlock());
                }
            }

            var actingFlow = new ActingFlow<TSource, TOutput>(actionBlock)
            {
                Registry = this.Registry,
                Label = this.Label
            };

            log.Trace("Broadcast operation initialized");
            return actingFlow;
        }

        public IRequestResponseFlow<TSource, TInput> Respond()
        {
            // Create a tail flow to broadcast flow results, only clients with specific correlation queries will get the results

            //todo: provide a deep copying cloning function for the broadcast block
            var broadcast = new BroadcastBlock<FlowContext<TInput>>(p => p);
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


        private Func<FlowContext<TInput>, FlowContext<TOutput>> GetCachingActionWrapper<TOutput>(Func<FlowContext<TInput>, TOutput> act, int capacity, int scale, ICachePolicy policy) where TOutput : class
        {
            return  input =>
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

                return new FlowContext<TOutput> {Id = Guid.NewGuid(), Value = output, Error = exception, Tail = input};
            };
        }
    }
}