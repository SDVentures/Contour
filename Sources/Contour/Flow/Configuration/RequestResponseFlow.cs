using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Handles the flow interactions in a request-response scenario.
    /// </summary>
    /// <typeparam name="TInput">The type of the flow input messages</typeparam>
    /// <typeparam name="TOutput">The type of the flow output messages</typeparam>
    public class RequestResponseFlow<TInput, TOutput> : IRequestResponseFlow<TInput, TOutput>
    {
        /// <summary>
        /// The tail block
        /// </summary>
        private readonly ISourceBlock<FlowContext<TOutput>> tailBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseFlow{TInput,TOutput}"/> class.
        /// </summary>
        /// <param name="tailBlock">
        /// The last block in the flow processing chain
        /// </param>
        public RequestResponseFlow(ISourceBlock<FlowContext<TOutput>> tailBlock)
        {
            this.tailBlock = tailBlock;
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
        /// Registers a new flow entry
        /// </summary>
        /// <returns>
        /// The <see cref="IFlowEntry{TInput}"/>.
        /// </returns>
        public IFlowEntry<TInput> Entry()
        {
            var head = (IMessageFlow<TInput, TInput>)this.Registry.Get(this.Label);
            return head.Entry();
        }

        /// <summary>
        /// Registers a new flow entry point with a callback
        /// </summary>
        /// <param name="callback">
        /// An action called by the flow if one has a response chain registered
        /// </param>
        /// <returns>
        /// The <see cref="IFlowEntry{TInput}"/>
        /// </returns>
        public IFlowEntry<TInput> Entry(Action<FlowContext<TOutput>> callback)
        {
            var head = (IMessageFlow<TInput, TInput>)this.Registry.Get(this.Label);
            var entry = head.Entry();

            var correlationQuery = new Predicate<FlowContext<TOutput>>(p => p.Head() == entry.Id);

            var action = new ActionBlock<FlowContext<TOutput>>(callback);
            this.tailBlock.LinkTo(action, correlationQuery);

            return entry;
        }
    }
}