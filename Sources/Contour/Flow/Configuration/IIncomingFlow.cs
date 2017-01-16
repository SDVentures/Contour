namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides an entry point for the incoming flow, enables the client code to post or send messages to the flow
    /// </summary>
    public interface IIncomingFlow
    {
        /// <summary>
        /// Describes a client subscription for the message flow with <paramref name="label"/> label and <typeparamref name="TOutput" /> type.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="capacity"></param>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        IActingFlow<TOutput> On<TOutput>(string label, int capacity = 1);
    }
}