using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    /// <summary>
    /// Represents a dataflow message block which can be used as a source in flow processing pipelines
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class SourceMessageBlock<TOutput> : MessageBlock, IReceivableSourceBlock<TOutput>
    {
        private readonly BufferBlock<TOutput> buffer;
        private readonly ISourceBlock<TOutput> source;

        /// <summary>
        /// Constructs a new instance of <see cref="SourceMessageBlock{TOutput}"/>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="capacity"></param>
        public SourceMessageBlock(string label, int capacity = 1)
        {
            buffer = new BufferBlock<TOutput>(new DataflowBlockOptions()
            {
                BoundedCapacity = capacity
            });

            source = buffer;
        }

        IDisposable ISourceBlock<TOutput>.LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return buffer.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Enables <paramref name="target"/> to consume a message from this source (commit phase)
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        /// <param name="messageConsumed"></param>
        /// <returns></returns>
        TOutput ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return source.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Reserves a message for <paramref name="target"/> for future processing (prepare phase)
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return source.ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Releases a previously reserved message (rollback phase)
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="target"></param>
        void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            source.ReleaseReservation(messageHeader, target);
        }

        /// <summary>
        /// Tries to receive an item using the <paramref name="filter"/> provided
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryReceive(Predicate<TOutput> filter, out TOutput item)
        {
            return buffer.TryReceive(filter, out item);
        }

        /// <summary>
        /// Tries to receive all available items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool TryReceiveAll(out IList<TOutput> items)
        {
            return buffer.TryReceiveAll(out items);
        }
    }
}