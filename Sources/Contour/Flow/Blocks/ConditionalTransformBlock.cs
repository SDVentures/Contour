using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    /// <summary>
    /// Transforms incoming messages and continues the flow if a specified condition is met.
    /// </summary>
    /// <typeparam name="TInput">
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// </typeparam>
    internal class ConditionalTransformBlock<TInput, TOutput>: MessageBlock, IPropagatorBlock<TInput, TOutput>
    {
        /// <summary>
        /// The incoming.
        /// </summary>
        private readonly IPropagatorBlock<TInput, TOutput> incoming;

        /// <summary>
        /// The outgoing.
        /// </summary>
        private readonly BroadcastBlock<TOutput> outgoing;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalTransformBlock{TInput,TOutput}"/> class.
        /// </summary>
        /// <param name="func">
        /// The func.
        /// </param>
        /// <param name="successPredicate">
        /// The success predicate.
        /// </param>
        public ConditionalTransformBlock(Func<TInput, TOutput> func, Predicate<TOutput> successPredicate)
            : this(func, successPredicate, new ExecutionDataflowBlockOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalTransformBlock{TInput,TOutput}"/> class.
        /// </summary>
        /// <param name="func">
        /// The func.
        /// </param>
        /// <param name="successPredicate">
        /// The success predicate.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        public ConditionalTransformBlock(Func<TInput, TOutput> func, Predicate<TOutput> successPredicate, ExecutionDataflowBlockOptions options)
        {
            this.incoming = new TransformBlock<TInput, TOutput>(func, options);
            this.outgoing = new BroadcastBlock<TOutput>(output => output, options);

            this.incoming.LinkTo(this.outgoing, successPredicate);
        }

        /// <summary>
        /// The offer message.
        /// </summary>
        /// <param name="messageHeader">
        /// The message header.
        /// </param>
        /// <param name="messageValue">
        /// The message value.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="consumeToAccept">
        /// The consume to accept.
        /// </param>
        /// <returns>
        /// The <see cref="DataflowMessageStatus"/>.
        /// </returns>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source,
            bool consumeToAccept)
        {
            return this.incoming.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        /// <summary>
        /// The link to.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="linkOptions">
        /// The link options.
        /// </param>
        /// <returns>
        /// The <see cref="IDisposable"/>.
        /// </returns>
        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return this.outgoing.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// The consume message.
        /// </summary>
        /// <param name="messageHeader">
        /// The message header.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="messageConsumed">
        /// The message consumed.
        /// </param>
        /// <returns>
        /// The <see cref="TOutput"/>.
        /// </returns>
        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return this.incoming.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// The reserve message.
        /// </summary>
        /// <param name="messageHeader">
        /// The message header.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return this.incoming.ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// The release reservation.
        /// </summary>
        /// <param name="messageHeader">
        /// The message header.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            this.incoming.ReleaseReservation(messageHeader, target);
        }
    }
}