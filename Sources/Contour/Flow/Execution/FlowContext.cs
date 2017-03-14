using System;

namespace Contour.Flow.Execution
{
    public class FlowContext<TValue> : FlowContext
    {
        public TValue Value { get; set; }

        public override Guid Head()
        {
            return Tail?.Head() ?? this.Id;
        }
    }

    public abstract class FlowContext
    {
        public Guid Id { get; set; }
        public FlowContext Tail { get; set; }
        public Exception Error { get; set; }

        public abstract Guid Head();
    }
}