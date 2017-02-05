using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    public interface IFlowFactory
    {
        IMessageFlow<TInput> Create<TInput>(string transportName);
    }
}