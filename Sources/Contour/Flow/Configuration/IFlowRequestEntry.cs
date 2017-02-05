using System;

namespace Contour.Flow.Configuration
{
    public interface IFlowRequestEntry<TOutput>
    {
        IFlowEntry<TIn> OnRequest<TIn>(string id, Predicate<TOutput> correlationQuery, Action<TOutput> callback);
    }
}