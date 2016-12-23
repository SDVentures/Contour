using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public interface ISourceProvider<out TOutput>
    {
        ISourceBlock<TOutput> Source();
    }
}