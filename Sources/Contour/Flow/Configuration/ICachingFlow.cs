using System;

namespace Contour.Flow.Configuration
{
    public interface ICachingFlow<TOutput>: IFlow
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
    }
}