using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    internal interface IFlowTransportProvider
    {
        IFlowTransport Get(IFlowTransportConfiguration configuration);
    }
}
