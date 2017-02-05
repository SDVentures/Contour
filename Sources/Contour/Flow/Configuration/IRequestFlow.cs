namespace Contour.Flow.Configuration
{
    public interface IRequestFlow<TInput> : IFlowEntry<TInput>, IFlowRegistryItem
    {
    }
}