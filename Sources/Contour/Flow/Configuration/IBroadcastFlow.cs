using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    public interface IBroadcastFlow<TSource>
    {
        IFlowRegistry Registry { set; }

        IActingFlowConcatenation<TSource, FlowContext<TIn, TOut>> Broadcast<TIn, TOut>(string label = null, int capacity = 1, int scale = 1);
    }
}