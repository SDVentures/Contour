using System;

namespace Contour.Flow.Configuration
{
    public interface ICachingFlow<TInput>: IFlow
    {
        /// <summary>
        /// Cache the flow directed to the initiator for <paramref name="timeSpan"/>
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        IOutgoingFlow<TInput> Respond();
    }
}