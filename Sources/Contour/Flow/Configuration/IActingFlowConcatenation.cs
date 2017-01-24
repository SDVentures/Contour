using System;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    public interface IActingFlowConcatenation<TInput>: IOutgoingFlow
    {
        IActingFlow<ActionContext<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);

        ITerminatingFlow Act(Action<TInput> act, int capacity = 1, int scale = 1);
    }
}