using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Makes a flow addressable by label and provides a way for the caller to check if the flow can be used as a target of specific type.
    /// </summary>
    public interface IFlowTarget
    {
        /// <summary>
        /// Flow label
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Returns a flow block which can be used as a target of specific type.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        ITargetBlock<TOutput> AsTarget<TOutput>();
    }
}