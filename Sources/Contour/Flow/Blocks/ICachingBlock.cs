using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;

namespace Contour.Flow.Blocks
{
    public interface ICachingBlock<TIn, TOut> where TOut : class
    {
        IDisposable LinkTo(ITargetBlock<FlowContext<TIn, TOut>> target, DataflowLinkOptions linkOptions);

        IDisposable MissLinkTo(ITargetBlock<FlowContext<TIn, TOut>> target, DataflowLinkOptions linkOptions);
    }
}