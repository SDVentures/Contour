using System;

namespace Contour.Flow.Configuration
{
    public interface IResponseFlow<out TOutput> : IFlowRegistryItem
    {
        IFlowEntry<TInput> Entry<TInput>(Action<TOutput> callback);
    }
}