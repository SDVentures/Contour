using System;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A message flow which enables user actions
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IActingFlowConcatenation<TInput>: IOutgoingFlow<TInput>
    {
        IActingFlow<ActionContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);

        ITailFlow<TInput> Act(Action<TInput> act, int capacity = 1, int scale = 1);
    }
}