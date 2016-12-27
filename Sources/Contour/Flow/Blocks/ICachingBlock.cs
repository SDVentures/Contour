using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public interface ICachingBlock<TIn, TOut> where TOut : class
    {
        IDisposable LinkMissed(ITargetBlock<Tuple<TIn, TOut>> target, DataflowLinkOptions linkOptions);
    }
}