using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Blocks
{
    public interface ICachingBlock<TIn, TOut> where TOut : class
    {
        IDisposable LinkTo(ITargetBlock<Tuple<TIn, TOut>> target, DataflowLinkOptions linkOptions);

        IDisposable MissLinkTo(ITargetBlock<Tuple<TIn, TOut>> target, DataflowLinkOptions linkOptions);
    }
}