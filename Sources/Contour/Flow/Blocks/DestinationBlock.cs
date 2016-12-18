using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public class DestinationBlock<TInput> : MessageBlock, ITargetBlock<TInput>
    {
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source,
            bool consumeToAccept)
        {
            throw new NotImplementedException();
        }
    }
}