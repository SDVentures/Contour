using System;

namespace Contour.Flow.Configuration
{
    public interface IActionableFlow: IOutgoingFlow
    {
        IActionableFlow Act<TInput, TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);
    }
}