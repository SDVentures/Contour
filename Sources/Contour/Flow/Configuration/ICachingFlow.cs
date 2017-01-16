using System;
using Contour.Caching;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    internal interface ICachingFlow<out TOutput>
    {
        /// <summary>
        /// Direct the flow using the routing provided by the flow source
        /// </summary>
        /// <returns></returns>
        IOutgoingFlow<TOutput> Respond();

        /// <summary>
        /// Direct the flow forward using <paramref name="label"/> as the destination
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        IOutgoingFlow<TOutput> Forward(string label);

        /// <summary>
        /// Cache the flow using the specified <paramref name="policy"/>
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="policy"></param>
        /// <returns></returns>
        ICachingFlow<TOut> Cache<TIn, TOut>(ICachePolicy policy) where TOut : class;
    }
}