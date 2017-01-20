using System;

namespace Contour.Flow.Execution
{
    public class ActionContext<TIn, TOut>
    {
        public TIn Arg { get; set; }

        public TOut Result { get; set; }

        public Exception Error { get; set; }
    }
}