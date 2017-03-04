namespace Contour.Flow.Configuration
{
    /// <summary>
    /// An atomic item of the flow
    /// </summary>
    public interface IFlowItem<in T>
    {
        /// <summary>
        /// Flow originating label
        /// </summary>
        string Label { set; }

        /// <summary>
        /// Flow message type
        /// </summary>
        T Type { set; }
    }
}