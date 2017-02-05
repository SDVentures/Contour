using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a way for the caller to check if the flow can be used as a target of specific type.
    /// </summary>
    public interface IFlowTarget<TInput>
    {
        /// <summary>
        /// Returns a flow block which can be used as a target of specific type.
        /// </summary>
        /// <returns></returns>
        ITargetBlock<TInput> AsTarget();
    }
}