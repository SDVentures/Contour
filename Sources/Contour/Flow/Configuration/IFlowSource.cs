using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public interface IFlowSource<TOutput>
    {
        ISourceBlock<TOutput> AsSource();
    }
}