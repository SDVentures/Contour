using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    /// <summary>
    /// Provides a building block to embed message handling into task dataflow based pipelines
    /// </summary>
    public abstract class MessageBlock: IDataflowBlock
    {
        private readonly TaskCompletionSource<bool>  completionSource = new TaskCompletionSource<bool>();

        public virtual void Complete()
        {
            completionSource.SetResult(true);
        }

        public virtual void Fault(Exception exception)
        {
            completionSource.SetException(exception);
        }

        public Task Completion => completionSource.Task;
    }
}