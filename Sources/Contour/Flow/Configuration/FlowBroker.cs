using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Contour.Flow.Configuration
{
    internal class FlowBroker : IFlowFactory, IFlowTransportRegistry, IFlowRegistry
    {
        private readonly ConcurrentDictionary<string, IFlowTransport> transports = new ConcurrentDictionary<string, IFlowTransport>();
        private readonly ConcurrentBag<IFlowTarget> flows = new ConcurrentBag<IFlowTarget>();

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
                flow.Registry = registry;

                return flow;
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to get a message flow [{transportName}]", ex);
            }
        }

        IEnumerable<IFlowTarget> IFlowRegistry.Get<TOutput>()
        {
            var results = flows.Where(ft => ft.AsTarget<TOutput>() != null);
            return results;
        }

        void IFlowRegistry.Add(IFlowTarget flow)
        {
            flows.Add(flow);
        }
    }
}