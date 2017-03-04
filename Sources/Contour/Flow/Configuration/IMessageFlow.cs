using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A generic flow of messages
    /// </summary>
    public interface IMessageFlow<TInput> : IFlowAccessor<TInput>, IFlowRegistryItem
    {
        /// <summary>
        /// Describes a client subscription for the message flow with <paramref name="label"/> label
        /// </summary>
        /// <param name="label">A flow label used to identify an incoming flow</param>
        /// <param name="capacity">Maximum capacity of the incoming flow buffer</param>
        /// <returns></returns>
        IActingFlow<TInput, TInput> On(string label, int capacity = 1);
    }
}