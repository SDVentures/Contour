using Contour.Flow.Configuration;
using Contour.Flow.Execution;

namespace Contour.Flow.Transport
{
    public interface IFlowTransport
    {
        IMessageFlow<TSource, FlowContext<TSource>> CreateFlow<TSource>();
    }
}