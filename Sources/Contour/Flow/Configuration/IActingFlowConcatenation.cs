using System;
using Contour.Caching;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A message flow which enables user actions
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TSource">The source flow message type</typeparam>
    public interface IActingFlowConcatenation<TSource, TInput>: IOutgoingFlow<TSource, TInput>
    {
        IActingFlow<TSource, FlowContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput: class;

        ITerminatingFlow Act(Action<TInput> act, int capacity = 1, int scale = 1);
    }
}