using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides an in-memory flow implementation
    /// </summary>
    public class LocalMessageFlow<TInput> :  IMessageFlow<TInput>
    {
        private readonly ILog log = LogManager.GetLogger<LocalMessageFlow<TInput>>();
        private IPropagatorBlock<TInput, TInput> buffer;
        private IIncomingFlow<TInput> incomingFlow;

        /// <summary>
        /// Flow label
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Flow items type
        /// </summary>
        public Type Type => typeof (TInput);

        /// <summary>
        /// A flow registry which provides flow coordination in request-response and broadcasting scenarios.
        /// </summary>
        public IFlowRegistry Registry { private get; set; }

        /// <summary>
        /// Flow transport reference
        /// </summary>
        public IFlowTransport Transport { get; }

        /// <summary>
        /// Describes a flow entry point
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public IFlowEntry<TInput> Entry => incomingFlow;

        /// <summary>
        /// Creates a new local message flow
        /// </summary>
        /// <param name="transport"></param>
        public LocalMessageFlow(IFlowTransport transport)
        {
            this.Transport = transport;
        }

        /// <summary>
        /// Registers a new flow of <typeparamref name="TInput"/> typed items.
        /// </summary>
        /// <param name="label">Flow label</param>
        /// <param name="capacity">Specifies the maximum capacity of the flow pipeline</param>
        /// <returns></returns>
        public IActingFlow<TInput> On(string label, int capacity = 1)
        {
            if (buffer != null)
                throw new FlowConfigurationException($"Flow [{Label}] has already been configured");

            this.Label = label;
            buffer = new BufferBlock<TInput>(new DataflowBlockOptions() {BoundedCapacity = capacity});
            var flow = new ActingFlow<TInput>(buffer)
            {
                Registry = Registry,
                Transport = Transport
            };

            incomingFlow = new LocalIncomingFlow<TInput>(buffer);
            return flow;
        }

        ITargetBlock<TInput> IFlowTarget<TInput>.AsTarget()
        {
            EnsureSourceConfigured();
            return buffer;
        }

        private void EnsureSourceConfigured()
        {
            if (buffer == null)
            {
                throw new FlowConfigurationException($"Flow [{Label}] is not yet configured, call On<> method first.");
            }
        }
    }
}