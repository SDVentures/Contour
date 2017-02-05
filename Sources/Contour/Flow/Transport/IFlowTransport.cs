using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    public interface IFlowTransport
    {
        IMessageFlow<TOutput> CreateFlow<TOutput>();

        string GetTailLabel(string sourceLabel);
    }
}