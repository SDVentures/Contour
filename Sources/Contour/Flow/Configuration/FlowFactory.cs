using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Contour.Flow.Configuration
{
    internal class FlowFactory : IFlowFactory
    {
        private readonly ConcurrentDictionary<string, IFlowTransport> transports = new ConcurrentDictionary<string, IFlowTransport>();
        private readonly ConcurrentBag<IMessageFlow> flows = new ConcurrentBag<IMessageFlow>();
        
        public void RegisterTransport(string name, IFlowTransport transport)
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

        public IMessageFlow Create(string transportName)
        {
            try
            {
                var transport = transports[transportName];
                var flow = transport.CreateFlow();
                flows.Add(flow);
                return flow;
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to get a message flow [{transportName}]", ex);
            }
        }

        public IFlowEntry Get(string id)
        {
            try
            {
                return flows.First(f => f.Id == id);
            }
            catch (Exception ex)
            {
                throw new FlowConfigurationException($"Failed to get a flow entry [{id}]", ex);
            }
        }
    }
}