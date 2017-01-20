using System;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;

namespace Contour.Flow.Blocks
{
    /// <summary>
    /// Caches the output of a source block using a <see cref="ICachePolicy"/>. Splits the outgoing flow in two streams of hits and misses
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class CachingBlock<TIn, TOut> : MessageBlock, IPropagatorBlock<ActionContext<TIn, TOut>, ActionContext<TIn, TOut>>, ICachingBlock<TIn, TOut> where TOut: class
    {
        private readonly ILog log = LogManager.GetLogger<CachingBlock<TIn, TOut>>();
        private readonly TransformBlock<ActionContext<TIn, TOut>, ActionContext<TIn, TOut>> cacheTransform;
        private readonly IPropagatorBlock<ActionContext<TIn, TOut>, ActionContext<TIn, TOut>> propagator;
        private readonly ICachePolicy policy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        public CachingBlock(ICachePolicy policy):this(policy, new ExecutionDataflowBlockOptions())
        {
        }

        /// <summary>
        /// Constructs a new caching block using the specified caching policy
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="options"></param>
        public CachingBlock(ICachePolicy policy, ExecutionDataflowBlockOptions options)
        {
            this.policy = policy;

            cacheTransform = new TransformBlock<ActionContext<TIn, TOut>, ActionContext<TIn, TOut>>(new Func<ActionContext<TIn, TOut>, ActionContext<TIn, TOut>>(CachingFunction), options);
            propagator = cacheTransform;
        }

        /// <summary>
        /// Receives a message from the source block
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="messageValue"></param>
        /// <param name="source"></param>
        /// <param name="consumeToAccept"></param>
        /// <returns></returns>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, ActionContext<TIn, TOut> messageValue, ISourceBlock<ActionContext<TIn, TOut>> source,
            bool consumeToAccept)
        {
            var status = propagator.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
            log.Debug(new { name = nameof(OfferMessage), header = messageHeader, message = messageValue, source, consumeToAccept, status });

            return status;
        }

        /// <summary>
        /// Links cache hits output to the target <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="linkOptions"></param>
        /// <returns></returns>
        public IDisposable LinkTo(ITargetBlock<ActionContext<TIn, TOut>> target, DataflowLinkOptions linkOptions)
        {
            return cacheTransform.LinkTo(target, linkOptions, item => item.Result != default(TOut));
        }
        
        /// <summary>
        /// Links cache misses output to the target <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="linkOptions"></param>
        /// <returns></returns>
        public IDisposable MissLinkTo(ITargetBlock<ActionContext<TIn, TOut>> target, DataflowLinkOptions linkOptions)
        {
            return cacheTransform.LinkTo(target, linkOptions, item => item.Result == default(TOut));
        }

        /// <summary>
        /// Completes a message consumption as an atomic operation
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        /// <param name="messageConsumed"></param>
        /// <returns></returns>
        public ActionContext<TIn, TOut> ConsumeMessage(DataflowMessageHeader messageHeader,
            ITargetBlock<ActionContext<TIn, TOut>> target, out bool messageConsumed)
        {
            var message = propagator.ConsumeMessage(messageHeader, target, out messageConsumed);
            log.Debug(new {name = nameof(ConsumeMessage), header = messageHeader, target, messageConsumed});

            return message;
        }

        /// <summary>
        /// Reserves a message for further consumption as an atomic operation
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<ActionContext<TIn, TOut>> target)
        {
            var result = propagator.ReserveMessage(messageHeader, target);
            log.Debug(new {name = nameof(ReserveMessage), header = messageHeader, target, result});

            return result;
        }

        /// <summary>
        /// Releases a message consumption reservation
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<ActionContext<TIn, TOut>> target)
        {
            propagator.ReleaseReservation(messageHeader, target);
            log.Debug(new {name = nameof(ReleaseReservation), header = messageHeader, target});
        }

        private ActionContext<TIn, TOut> CachingFunction(ActionContext<TIn, TOut> @in)
        {
            var key = policy.KeyProvider.Get(@in.Arg);
            log.Debug(new {name = nameof(CachingFunction), key});
            
            if (@in.Result != default(TOut))
            {
                policy.CacheProvider.Put(key, @in.Result, policy.Period);
                log.Debug($"An output with key=[{key}] has been cached for [{policy.Period}]");

                return @in;
            }

            // some implementations can throw exceptions on cache miss
            var value = default(TOut);
            try
            {
                value = policy.CacheProvider.Get<TOut>(key);
                log.Debug(value != default(TOut) ? $"Cache hit: key=[{key}]" : $"Cache miss: key=[{key}]");
            }
            catch(Exception ex)
            {
                log.Debug($"Cache miss or failure: key=[{key}]", ex);
            }

            return new ActionContext<TIn, TOut> {Arg = @in.Arg, Result = value};
        }
    }
}
