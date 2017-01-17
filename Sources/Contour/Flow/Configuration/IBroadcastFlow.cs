using System;

namespace Contour.Flow.Configuration
{
    public interface IBroadcastFlow
    {
        IActingFlowConcatenation<Tuple<TIn, TOut>> Broadcast<TIn, TOut>(string label = null, int capacity = 1, int scale = 1);
    }
}