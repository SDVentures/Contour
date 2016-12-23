using System;

namespace Contour.Flow.Configuration
{
    public interface IActingFlow<out TInput> : IFlow
    {
        IActingFlow<TOutput> Act<TOutput>(Func<TInput, TOutput> act, int capacity = 1, int scale = 1);

        /// <summary>
        /// Directs the flow to the destination specified by the incoming message.
        /// </summary>
        /// <returns></returns>
        IOutgoingFlow<TInput> Respond();

        ICachingFlow<TInput> Cache(TimeSpan duration);

        /// <summary>
        /// Forwards the flow with the specified <paramref name="label"/>
        /// </summary>
        /// <returns></returns>
        IOutgoingFlow<TInput> Forward(string label);
    }
}