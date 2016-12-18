using System;

namespace Contour.Flow.Configuration
{
    public interface IOutgoingFlow
    {
        /// <summary>
        /// Response on a flow message based on the <paramref name="responder"/> value
        /// </summary>
        /// <returns></returns>
        ICachebleFlow Respond<TInput, TOutput>(Func<TInput, TOutput> responder);

        /// <summary>
        /// Response on a flow message
        /// </summary>
        /// <returns></returns>
        ICachebleFlow Respond();

        /// <summary>
        /// Instruct the flow to continue with the specified <paramref name="label"/>
        /// </summary>
        /// <returns></returns>
        IMessageFlow Forward(string label);

        //todo: respond and forward ?
    }
}