using System;

namespace Contour.Flow.Configuration
{
    public interface IActingFlowConcatenation<TInput>: IOutgoingFlow
    {
        IActingFlow<Tuple<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);
    }
}