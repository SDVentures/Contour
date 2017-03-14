using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Contour.Flow.Execution;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class FlowBroker : IFlowFactory, IFlowTransportRegistry, IFlowRegistry
    {
        private readonly ConcurrentDictionary<string, IFlowTransport> transports = new ConcurrentDictionary<string, IFlowTransport>();
        private readonly ConcurrentBag<IFlowRegistryItem> flows = new ConcurrentBag<IFlowRegistryItem>();

        public void Register(string name, IFlowTransport transport)
        {
            try
            {
                transports.TryAdd(name, transport);
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to register flow transport [{name}, {transport}]", ex);
            }
        }

        public IMessageFlow<TInput, TInput> Create<TInput>(string transportName)
        {
            try
            {
                var transport = transports[transportName];
                var flow = transport.CreateFlow<TInput>();

                var registry = (IFlowRegistry) this;
                registry.Add(flow);
                flow.Registry = registry;

                return flow;
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to create a message flow", ex);
            }
        }

        public IEnumerable<IFlowRegistryItem> GetAll<TInput>()
        {
            return flows.Where(f => f.Type == typeof (TInput));
        }

        public IEnumerable<IFlowRegistryItem> GetAll()
        {
            return flows;
        }

        public IFlowRegistryItem Get(string label)
        {
            return flows.Single(f => f.Label == label);
        }
        
        public void Add(IFlowRegistryItem flow)
        {
            flows.Add(flow);
        }

    }
}