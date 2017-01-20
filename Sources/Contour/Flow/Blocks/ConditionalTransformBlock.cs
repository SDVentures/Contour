using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    /// <summary>
    /// Transforms incoming messages and continues the flow if a specified condition is met.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal class ConditionalTransformBlock<TInput, TOutput>: MessageBlock, IPropagatorBlock<TInput, TOutput>
    {
        private readonly IPropagatorBlock<TInput, TOutput> incoming;
        private readonly BufferBlock<TOutput> outgoing;

        public ConditionalTransformBlock(Func<TInput, TOutput> func, Predicate<TOutput> predicate)
            : this(func, predicate, new ExecutionDataflowBlockOptions())
        {
        }

        public ConditionalTransformBlock(Func<TInput, TOutput> func, Predicate<TOutput> predicate, ExecutionDataflowBlockOptions options)
        {
            incoming = new TransformBlock<TInput, TOutput>(func, options);
            outgoing = new BufferBlock<TOutput>(options);

            incoming.LinkTo(outgoing, predicate);
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source,
            bool consumeToAccept)
        {
            return incoming.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return outgoing.LinkTo(target, linkOptions);
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return incoming.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return incoming.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            incoming.ReleaseReservation(messageHeader, target);
        }
    }
}