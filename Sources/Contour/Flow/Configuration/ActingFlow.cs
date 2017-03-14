using System;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Caching;
using Contour.Flow.Blocks;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    using System.Linq;

    /// <summary>
    /// The flow which can be used to process messages, broadcast the results, respond to the flow caller or forward the result to a new destination
    /// </summary>
    /// <typeparam name="TSource">The type of the incoming messages preserved by the flow
    /// </typeparam>
    /// <typeparam name="TInput">The type of the incoming messages
    /// </typeparam>
    internal class ActingFlow<TSource, TInput> : IActingFlow<TSource, TInput>, IActingFlowConcatenation<TSource, TInput>
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("ActingFlow");

        /// <summary>
        /// The source block with the incoming messages
        /// </summary>
        private readonly ISourceBlock<FlowContext<TInput>> sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActingFlow{TSource,TInput}"/> class.
        /// </summary>
        /// <param name="sourceBlock">
        /// The source.
        /// </param>
        public ActingFlow(ISourceBlock<FlowContext<TInput>> sourceBlock)
        {
            this.sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Gets or sets the flow registry
        /// </summary>
        public IFlowRegistry Registry { private get; set; }

        /// <summary>
        /// Gets or sets the flow label
        /// </summary>
        public string Label { private get; set; }

        /// <summary>
        /// Performs a user specified action on the flow and pushes the result and any errors down the flow; the result can be optionally cached
        /// </summary>
        /// <param name="act">
        /// The action which should be executed on the incoming flow
        /// </param>
        /// <param name="capacity">
        /// The maximum capacity of the flow chain defined by the number of unprocessed messages
        /// </param>
        /// <param name="scale">
        /// The maximum parallelism factor which can be achieved by the flow execution runtime
        /// </param>
        /// <param name="policy">
        /// The result caching policy
        /// </param>
        /// <typeparam name="TOutput">
        /// The type of the flow output messages
        /// </typeparam>
        /// <returns>
        /// The <see cref="IActingFlow{TSource, TOutput}"/>.
        /// </returns>
        public IActingFlow<TSource, TOutput> Act<TOutput>(Func<FlowContext<TInput>, TOutput> act, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput : class
        {
            var actionWrapper = this.GetCachingActionWrapper(act, capacity, scale, policy);

            Predicate<FlowContext<TOutput>> successPredicate = context => context.Error == null;

            var action = new ConditionalTransformBlock<FlowContext<TInput>, FlowContext<TOutput>>(
                actionWrapper,
                successPredicate,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            this.sourceBlock.LinkTo(action);

            var flow = new ActingFlow<TSource, TOutput>(action)
            {
                Registry = this.Registry,
                Label = this.Label
            };
            return flow;
        }

        /// <summary>
        /// Performs a user specified action on the flow
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="capacity">
        /// The maximum capacity of the flow chain defined by the number of unprocessed messages
        /// </param>
        /// <param name="scale">
        /// The maximum parallelism factor which can be achieved by the flow execution runtime
        /// </param>
        /// <returns>
        /// The <see cref="ITerminatingFlow"/>.
        /// </returns>
        public ITerminatingFlow Act(Action<FlowContext<TInput>> act, int capacity = 1, int scale = 1)
        {
            Action<FlowContext<TInput>> actionWrapper = input =>
            {
                try
                {
                    this.log.Trace($"Executing an action [{act}] on [{input}] with capacity={capacity} at scale={scale}");
                    act(input);
                    this.log.Trace($"Action [{act}] execution completed successfully");
                }
                catch (Exception ex)
                {
                    this.log.Error($"Action [{act}] execution failed due to {ex.Message}");
                }
            };

            var action = new ActionBlock<FlowContext<TInput>>(
                actionWrapper,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            this.sourceBlock.LinkTo(action);
            var flow = new TerminatingFlow();
            return flow;
        }

        /// <summary>
        /// Performs a user defined action on the flow and broadcasts the results to all flows registered in the flow broker; the result can be optionally cached
        /// </summary>
        /// <param name="act">
        /// The action which should be executed on the incoming flow
        /// </param>
        /// <param name="label">
        /// The flow label
        /// </param>
        /// <param name="capacity">
        /// The maximum capacity of the flow chain defined by the number of unprocessed messages
        /// </param>
        /// <param name="scale">
        /// The maximum parallelism factor which can be achieved by the flow execution runtime
        /// </param>
        /// <param name="policy">
        /// The result caching policy
        /// </param>
        /// <typeparam name="TOutput">
        /// The type of the flow output messages
        /// </typeparam>
        /// <returns>
        /// The <see cref="IActingFlowConcatenation{TSource, TOutput}"/>.
        /// </returns>
        public IActingFlowConcatenation<TSource, TOutput> Broadcast<TOutput>(Func<FlowContext<TInput>, TOutput> act, string label = null, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput : class
        {
            var actionWrapper = this.GetCachingActionWrapper(act, capacity, scale, policy);
            Predicate<FlowContext<TOutput>> resultPropagationPredicate = context => context.Error == null;

            var actionBlock = new ConditionalTransformBlock<FlowContext<TInput>, FlowContext<TOutput>>(
                actionWrapper,
                resultPropagationPredicate,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = capacity,
                    MaxDegreeOfParallelism = scale
                });

            this.sourceBlock.LinkTo(actionBlock);
            this.log.Trace($"A source block [{this.sourceBlock}] has been linked to action block [{actionBlock}]");

            // todo: provide a deep copying cloning function for broadcast
            var broadcastBlock = new BroadcastBlock<FlowContext<TOutput>>(
                p => p,
                new DataflowBlockOptions()
                {
                    BoundedCapacity = capacity
                });

            actionBlock.LinkTo(broadcastBlock);
            this.log.Trace($"Action block results will be broadcasted via [{broadcastBlock}]");

            // Get all flows by specific type from the registry (flow label is irrelevant here due to possible flow items type casting errors)
            // todo: get flows by label if provided
            this.log.Trace($"Searching for the flow accepting [{nameof(TOutput)}] as input...");
            var flows = this.Registry.GetAll().OfType<IMessageFlow<TOutput, TOutput>>().ToList();
            this.log.Trace($"Found {flows.Count} flows to broadcast to");

            foreach (var head in flows)
            {
                this.log.Trace($"Linking broadcast block to [{head}]");
                broadcastBlock.LinkTo(head.Entry().AsBlock());
            }

            var actingFlow = new ActingFlow<TSource, TOutput>(actionBlock)
            {
                Registry = this.Registry,
                Label = this.Label
            };

            this.log.Trace("Broadcast operation initialized");
            return actingFlow;
        }

        /// <summary>
        /// Configures the flow to send the results back to the caller
        /// </summary>
        /// <returns>
        /// The <see cref="IRequestResponseFlow{TSource, TInput}"/>.
        /// </returns>
        public IRequestResponseFlow<TSource, TInput> Respond()
        {
            // Create a tail flow to broadcast flow results, only clients with specific correlation queries will get the results

            // todo: provide a deep copying cloning function for the broadcast block
            var broadcast = new BroadcastBlock<FlowContext<TInput>>(p => p);
            this.sourceBlock.LinkTo(broadcast);

            var flow = new RequestResponseFlow<TSource, TInput>(broadcast)
            {
                Registry = this.Registry,
                Label = this.Label
            };
            return flow;
        }

        /// <summary>
        /// Configures the flow to forward messages to a flow defined by a label
        /// </summary>
        /// <param name="label">
        /// The label of the flow to forward the messages to
        /// </param>
        /// <returns>
        /// The <see cref="IRequestResponseFlow{TSource, TInput}"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IRequestResponseFlow<TSource, TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets a function which wraps a user defined function with caching and context handling logic
        /// </summary>
        /// <param name="act">
        /// A user defined action to be executed on the flow
        /// </param>
        /// <param name="capacity">
        /// The maximum capacity of the flow chain defined by the number of unprocessed messages
        /// </param>
        /// <param name="scale">
        /// The maximum parallelism factor which can be achieved by the flow execution runtime
        /// </param>
        /// <param name="policy">
        /// The result caching policy
        /// </param>
        /// <typeparam name="TOutput">The type of the output messages
        /// </typeparam>
        /// <returns>
        /// The <see cref="Func{FlowContext, FlowContext}"/>.
        /// </returns>
        private Func<FlowContext<TInput>, FlowContext<TOutput>> GetCachingActionWrapper<TOutput>(
            Func<FlowContext<TInput>, TOutput> act, int capacity, int scale, ICachePolicy policy) where TOutput : class
        {
            return input =>
            {
                var output = default(TOutput);
                Exception exception = null;

                try
                {
                    this.log.Debug($"Executing [{act}] on [{input}] at scale={scale}, capacity={capacity}");

                    if (policy != null)
                    {
                        this.log.Debug($"Using [{policy}] as cache policy");

                        var key = policy.KeyProvider.Get(input);
                        this.log.Debug($"A key [{key}] for [{input}] argument acquired");

                        try
                        {
                            output = policy.CacheProvider.Get<TOutput>(key);
                            this.log.Debug(output != null ? $"Cache hit: key=[{key}]" : $"Cache miss: key=[{key}]");
                        }
                        catch (Exception ex)
                        {
                            this.log.Error($"Failed to get a cached output for action [{act}] due to {ex.Message}");
                            exception = ex;
                        }

                        if (output == null)
                        {
                            this.log.Trace($"Executing the action [{act}]");
                            output = act(input);
                            this.log.Trace($"Action [{act}] executed successfully, caching the result");

                            policy.CacheProvider.Put(key, output, policy.Period);
                            this.log.Trace(
                                $"Action [{act}] execution result [{output}] has been cached with [{policy}] for [{policy.Period}]");
                        }
                    }
                    else
                    {
                        this.log.Trace($"No caching policy is provided, executing the action [{act}]");
                        output = act(input);
                        this.log.Trace($"Action [{act}] executed successfully");
                    }
                }
                catch (Exception ex)
                {
                    this.log.Error(
                        $"Action [{act}({input}, capacity:{capacity}, scale:{scale})] execution failed due to {ex.Message}");
                    exception = ex;
                }

                return new FlowContext<TOutput>
                {
                    Id = Guid.NewGuid(),
                    Value = output,
                    Error = exception,
                    Tail = input
                };
            };
        }
    }
}