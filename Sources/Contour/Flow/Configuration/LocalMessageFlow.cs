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
    public class LocalMessageFlow<TSource> : IMessageFlow<TSource, FlowContext<TSource>>
    {
        private readonly ILog log = LogManager.GetLogger<LocalMessageFlow<TSource>>();
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
        /// Creates a new local message flow
        /// </summary>
        /// <param name="transport"></param>
        public LocalMessageFlow(IFlowTransport transport)
        {
            this.Transport = transport;
        }

        /// <summary>
        /// Registers a new flow of <typeparamref name="TSource"/> typed items.
        /// </summary>
        /// <param name="label">Flow label</param>
        /// <param name="capacity">Specifies the maximum capacity of the flow pipeline</param>
        /// <returns></returns>
        IActingFlow<TSource, FlowContext<TSource>> IMessageFlow<TSource, FlowContext<TSource>>.On(string label, int capacity)
        {
            if (buffer != null)
                throw new FlowConfigurationException($"Flow [{Label}] has already been configured");

            this.Label = label;

            buffer =
                new BufferBlock<FlowContext<TSource>>(new ExecutionDataflowBlockOptions() { BoundedCapacity = capacity });

            var flow = new ActingFlow<TSource, FlowContext<TSource>>(buffer)
            {
                Registry = this.Registry,
                Label = this.Label
            };

            return flow;
        }
        
        public IFlowEntry<TSource> Entry()
        {
            var entry = new FlowEntry<TSource>(buffer);
            return entry;
        }
    }
}