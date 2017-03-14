using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// The flow broker.
    /// </summary>
    internal class FlowBroker : IFlowBroker
    {
        /// <summary>
        /// The transports.
        /// </summary>
        private readonly ConcurrentDictionary<string, IFlowTransport> transports = new ConcurrentDictionary<string, IFlowTransport>();

        /// <summary>
        /// The flows.
        /// </summary>
        private readonly ConcurrentBag<IFlowRegistryItem> flows = new ConcurrentBag<IFlowRegistryItem>();

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="transport">
        /// The transport.
        /// </param>
        /// <exception cref="FlowConfigurationException">
        /// </exception>
        public void Register(string name, IFlowTransport transport)
        {
            try
            {
                this.transports.TryAdd(name, transport);
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to register flow transport [{name}, {transport}]", ex);
            }
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="transportName">
        /// The transport name.
        /// </param>
        /// <typeparam name="TInput">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IMessageFlow"/>.
        /// </returns>
        /// <exception cref="FlowConfigurationException">
        /// </exception>
        public IMessageFlow<TInput, TInput> Create<TInput>(string transportName)
        {
            try
            {
                var transport = this.transports[transportName];
                var flow = transport.CreateFlow<TInput>();

                var registry = (IFlowRegistry)this;
                registry.Add(flow);
                flow.Registry = registry;

                return flow;
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to create a message flow", ex);
            }
        }

        /// <summary>
        /// The get all.
        /// </summary>
        /// <typeparam name="TInput">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<IFlowRegistryItem> GetAll<TInput>()
        {
            return this.flows.Where(f => f.Type == typeof (TInput));
        }

        /// <summary>
        /// The get all.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<IFlowRegistryItem> GetAll()
        {
            return this.flows;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="IFlowRegistryItem"/>.
        /// </returns>
        public IFlowRegistryItem Get(string label)
        {
            return this.flows.Single(f => f.Label == label);
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="flow">
        /// The flow.
        /// </param>
        public void Add(IFlowRegistryItem flow)
        {
            this.flows.Add(flow);
        }
    }
}