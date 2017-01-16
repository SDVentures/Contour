using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    internal class InMemoryFlowTransport: IInMemoryFlowTransport
    {
        public IMessageFlow CreateFlow()
        {
            return new InMemoryMessageFlow();
        }
    }
}