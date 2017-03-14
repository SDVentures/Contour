using System;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Flow.Execution;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides an in-memory flow implementation
    /// </summary>
    /// <typeparam name="TSource">
    /// </typeparam>
    public class LocalMessageFlow<TSource> : IMessageFlow<TSource, TSource>
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger<LocalMessageFlow<TSource>>();

        /// <summary>
        /// The buffer.
        /// </summary>
        private BufferBlock<FlowContext<TSource>> buffer;

        /// <summary>
        /// Flow label
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Flow items type
        /// </summary>
        public Type Type => typeof(TSource);

        /// <summary>
        /// A flow registry which provides flow coordination in request-response and broadcasting scenarios.
        /// </summary>
        public IFlowRegistry Registry { private get; set; }

        /// <summary>
        /// Flow transport reference
        /// </summary>
        public IFlowTransport Transport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalMessageFlow{TSource}"/> class. 
        /// Creates a new local message flow
        /// </summary>
        /// <param name="transport">
        /// </param>
        public LocalMessageFlow(IFlowTransport transport)
        {
            this.Transport = transport;
        }

        /// <summary>
        /// Registers a new flow of <typeparamref name="TSource"/> typed items.
        /// </summary>
        /// <param name="label">
        /// Flow label
        /// </param>
        /// <param name="capacity">
        /// Specifies the maximum capacity of the flow pipeline
        /// </param>
        /// <returns>
        /// The <see cref="IActingFlow"/>.
        /// </returns>
        public IActingFlow<TSource, TSource> On(string label, int capacity)
        {
            if (this.buffer != null)
            {
                throw new FlowConfigurationException($"Flow [{this.Label}] has already been configured");
            }

            this.Label = label;

            this.buffer =
                new BufferBlock<FlowContext<TSource>>(new ExecutionDataflowBlockOptions() { BoundedCapacity = capacity });

            var flow = new ActingFlow<TSource, TSource>(this.buffer)
            {
                Registry = this.Registry,
                Label = this.Label
            };

            return flow;
        }

        /// <summary>
        /// The entry.
        /// </summary>
        /// <returns>
        /// The <see cref="IFlowEntry{TSource}"/>.
        /// </returns>
        public IFlowEntry<TSource> Entry()
        {
            var entry = new FlowEntry<TSource>(this.buffer);
            return entry;
        }
    }
}