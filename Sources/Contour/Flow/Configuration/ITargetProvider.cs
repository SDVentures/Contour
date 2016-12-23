using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public interface ITargetProvider<in TInput>
    {
        ITargetBlock<TInput> Target();
    }
}