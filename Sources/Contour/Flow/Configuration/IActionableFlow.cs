using System;

namespace Contour.Flow.Configuration
{
    public interface IActionableFlow<TInput> : IOutgoingFlow
    {
        IActionableFlow<TOutput> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);
    }
}