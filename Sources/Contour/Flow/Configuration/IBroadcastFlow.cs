using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    public interface IBroadcastFlow
    {
        IFlowRegistry Registry { set; }

        IActingFlowConcatenation<ActionContext<TIn, TOut>> Broadcast<TIn, TOut>(string label = null, int capacity = 1, int scale = 1);
    }
}