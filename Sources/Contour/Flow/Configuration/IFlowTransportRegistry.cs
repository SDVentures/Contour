using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal interface IFlowTransportRegistry
    {
        void Register(string name, IFlowTransport transport);
    }
}