using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public interface IFlowTarget
    {
        ITargetBlock<TOutput> AsTarget<TOutput>();
    }
}