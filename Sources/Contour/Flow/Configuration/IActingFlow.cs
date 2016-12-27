using System;

namespace Contour.Flow.Configuration
{
    public interface IActingFlow<TInput> : ICachingFlow<TInput>
    {
        IActingFlow<Tuple<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);

        ICachingFlow<TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class;
    }
}