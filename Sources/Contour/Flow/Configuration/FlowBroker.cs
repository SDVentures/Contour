using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class FlowBroker : IFlowFactory, IFlowTransportRegistry, IFlowRegistry
    {
        private readonly ConcurrentDictionary<string, IFlowTransport> transports = new ConcurrentDictionary<string, IFlowTransport>();
        private readonly ConcurrentBag<IFlowRegistryItem> flows = new ConcurrentBag<IFlowRegistryItem>();

        void IFlowTransportRegistry.Register(string name, IFlowTransport transport)
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

        IMessageFlow IFlowFactory.Create(string transportName)
        {
            try
            {
                var transport = transports[transportName];
                var flow = transport.CreateFlow();
                var registry = (IFlowRegistry) this;
                registry.Add(flow);
                flow.Root = registry;

                return flow;
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to get a message flow [{transportName}]", ex);
            }
        }

        public IEnumerable<IFlowRegistryItem> Get<TOutput>()
        {
            var results = flows.Where(ft => ft.AsTarget<TOutput>() != null);
            return results;
        }

        IEnumerable<IFlowRegistryItem> IFlowRegistry.Get(string label)
        {
            var results = flows.Where(ft => ft.Label == label);
            return results;
        }

        IEnumerable<IFlowRegistryItem> IFlowRegistry.Get<TOutput>(string label)
        {
            var results = flows.Where(ft => ft.Label == label && ft.AsTarget<TOutput>() != null);
            return results;
        }

        void IFlowRegistry.Add(IFlowRegistryItem flow)
        {
            flows.Add(flow);
        }
    }
}