using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    public interface IFlowTransport
    {
        IMessageFlow CreateFlow();
    }
}