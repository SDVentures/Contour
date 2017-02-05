namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes a flow with no further processing in a pipeline. A flow can be accessible by the client to get the results.
    /// </summary>
    public interface ITailFlow<TOutput> : IFlowRequestEntry<TOutput>, IFlowRegistryItem
    {
    }
}