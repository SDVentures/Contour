using System;

namespace Contour.Flow.Configuration
{
    public interface IActionableFlow: IOutgoingFlow
    {
        IActionableFlow Act<TInput, TOutput>(Func<TInput, TOutput> act, int scale = 1);
    }
}