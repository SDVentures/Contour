using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public class CachingBlock<TInput> : MessageBlock, IPropagatorBlock<TInput, TInput>
    {
        public CachingBlock(TimeSpan duration)
        {
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source,
            bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<TInput> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public TInput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TInput> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TInput> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TInput> target)
        {
            throw new NotImplementedException();
        }
    }
}
