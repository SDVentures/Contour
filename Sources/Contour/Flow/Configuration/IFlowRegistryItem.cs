namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A flow item controlled by the flow registry
    /// </summary>
    public interface IFlowRegistryItem: IFlowTarget
    {
        /// <summary>
        /// Flow originating label
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Flow registry reference
        /// </summary>
        IFlowRegistry Root { set; }
    }
}