using System;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A message flow which enables user actions, caching intermediate event broadcasting and requests
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TSource">The source flow message type</typeparam>
    public interface IActingFlow<TSource, TInput> : IOutgoingFlow<TSource, TInput>, ICachingFlow<TSource>,
        IBroadcastFlow<TSource>
    {
        IActingFlow<TSource, FlowContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1,
            int scale = 1);

        ITerminatingFlow Act(Action<TInput> act, int capacity = 1, int scale = 1);
    }
}