using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    public interface IFlowTransportRegistry
    {
        void Register(string name, IFlowTransport transport);
    }
}