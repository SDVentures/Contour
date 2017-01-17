using System;

namespace Contour.Flow.Configuration
{
    public interface IActingFlow<TInput> : IOutgoingFlow, ICachingFlow, IBroadcastFlow, IFlowRegistryItem
    {
        IActingFlow<Tuple<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);
    }
}