using System;

namespace Contour.Flow.Configuration
{
    internal interface IActingFlow<TInput> : ICachingFlow<TInput>
    {
        IActingFlow<Tuple<TInput, TOutput>> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);

        IActingFlow<Tuple<TInput, TOutput>> Broadcast<TOutput>();
    }
}