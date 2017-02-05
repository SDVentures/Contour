using System;

namespace Contour.Flow.Configuration
{
    public interface IResponseFlow<out TOutput> : IFlowRegistryItem
    {
        IFlowEntry<TInput> OnResponse<TInput>(Action<TOutput> callback);
    }
}