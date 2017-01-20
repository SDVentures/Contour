using System;

namespace Contour.Flow.Execution
{
    public class RequestContext<TRequestArg, TReply>
    {
        public TRequestArg Arg { get; set; }

        public TReply Reply { get; set; }

        public Exception Error { get; set; }
    }
}