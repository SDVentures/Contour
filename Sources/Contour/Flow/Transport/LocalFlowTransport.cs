using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    internal class LocalFlowTransport: ILocalFlowTransport
    {
        public IMessageFlow CreateFlow()
        {
            return new LocalMessageFlow();
        }
    }
}