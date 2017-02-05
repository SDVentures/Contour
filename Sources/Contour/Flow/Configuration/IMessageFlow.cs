namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A generic flow of messages
    /// </summary>
    public interface IMessageFlow<TInput> : IIncomingFlow<TInput>, IFlowEntry<TInput>, IFlowTarget<TInput>, IFlowRegistryItem
    {
        
    }
}