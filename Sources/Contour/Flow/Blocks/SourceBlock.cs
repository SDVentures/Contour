using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public class SourceBlock<TOutput> : MessageBlock, IReceivableSourceBlock<TOutput>
    {
        private BufferBlock<TOutput> buffer; 

        public SourceBlock(string label, int capacity = 1)
        {
            buffer = new BufferBlock<TOutput>(new DataflowBlockOptions() {BoundedCapacity = capacity});

            //todo: initialize message transport infrastructure here
            throw new NotImplementedException();
        }

        IDisposable ISourceBlock<TOutput>.LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        TOutput ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        bool ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }

        public bool TryReceive(Predicate<TOutput> filter, out TOutput item)
        {
            throw new NotImplementedException();
        }

        public bool TryReceiveAll(out IList<TOutput> items)
        {
            throw new NotImplementedException();
        }
    }
}