using System;

namespace Contour.Flow.Configuration
{
    public interface ICachebleFlow: IFlow
    {
        /// <summary>
        /// Cache the flow redirected to the initiator for <paramref name="timeSpan"/>
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        IMessageFlow CacheFor(TimeSpan timeSpan);


        /// <summary>
        /// Do not cache the flowing messages
        /// </summary>
        /// <returns></returns>
        IMessageFlow NoCache();
    }
}