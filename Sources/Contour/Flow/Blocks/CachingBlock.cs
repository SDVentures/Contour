using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Configuration;

namespace Contour.Flow.Blocks
{
    public class CachingBlock<TIn, TOut> : MessageBlock, IPropagatorBlock<Tuple<TIn, TOut>, Tuple<TIn, TOut>>, ICachingBlock<TIn, TOut> where TOut: class
    {
        private readonly TransformBlock<TIn, Tuple<TIn, TOut>> cacheTransform;

        public CachingBlock(ICachePolicy policy)
        {
            cacheTransform = new TransformBlock<TIn, Tuple<TIn, TOut>>(new Func<TIn, Tuple<TIn, TOut>>(Transform));
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, Tuple<TIn, TOut> messageValue, ISourceBlock<Tuple<TIn, TOut>> source,
            bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<Tuple<TIn, TOut>> target, DataflowLinkOptions linkOptions)
        {
            return cacheTransform.LinkTo(target, linkOptions, item => item.Item2 != default(TOut));
        }
        
        public IDisposable LinkMissed(ITargetBlock<Tuple<TIn, TOut>> target, DataflowLinkOptions linkOptions)
        {
            return cacheTransform.LinkTo(target, linkOptions, item => item.Item2 == default(TOut));
        }

        public Tuple<TIn, TOut> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<TIn, TOut>> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<TIn, TOut>> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<TIn, TOut>> target)
        {
            throw new NotImplementedException();
        }

        private Tuple<TIn, TOut> Transform(TIn @in)
        {
            throw new NotImplementedException("Implement a cache handling function here");
        }
    }
}
