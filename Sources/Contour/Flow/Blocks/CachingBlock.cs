using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public class CachingBlock<TInput, TOutput> : MessageBlock, IPropagatorBlock<TInput, TOutput>
    {
        public CachingBlock(TimeSpan duration)
        {
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source,
            bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }
    }
}
