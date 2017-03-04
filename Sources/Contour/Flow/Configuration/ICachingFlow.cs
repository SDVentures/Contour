using System;
using Contour.Caching;

namespace Contour.Flow.Configuration
{
    public interface ICachingFlow<TSource>
    {
        /// <summary>
        /// Cache the flow using the specified <paramref name="policy"/>
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="policy"></param>
        /// <returns></returns>
        IOutgoingFlow<TSource, TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class;
    }
}