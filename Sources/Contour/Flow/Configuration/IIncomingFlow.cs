namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides an entry point for the incoming flow with specific label and payload type, enables the client code to post or send messages to the flow
    /// </summary>
    public interface IIncomingFlow: IFlowEntry
    {
        /// <summary>
        /// Describes a client subscription for the message flow with <paramref name="label"/> label and <typeparamref name="TOutput" /> type.
        /// </summary>
        /// <param name="label">A flow label used to identify an incoming flow</param>
        /// <param name="capacity">Maximum capacity of the incoming flow buffer</param>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        IActingFlow<TOutput> On<TOutput>(string label, int capacity = 1);
    }
}