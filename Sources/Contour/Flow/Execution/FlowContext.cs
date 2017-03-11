using System;

namespace Contour.Flow.Execution
{
    public class FlowContext<TInput, TOutput>: FlowContext<TInput>
    {
        public TOutput Out { get; set; }

        public override object GetOut()
        {
            return Out;
        }
    }

    public class FlowContext<TInput>: FlowContext
    {
        public TInput In { get; set; }

        public Exception Error { get; set; }

        public override object GetIn()
        {
            return In;
        }

        public override object GetOut()
        {
            throw new NotSupportedException();
        }
    }

    public abstract class FlowContext
    {
        public Guid Id { get; set; }

        public FlowContext Unwind()
        {
            var @in = this;
            Unwind(ref @in);
            return @in;
        }

        public abstract object GetIn();

        public abstract object GetOut();
        
        private static void Unwind(ref FlowContext ctx)
        {
            var @in = ctx.GetIn();
            if (@in is FlowContext)
            {
                ctx = @in as FlowContext;
                Unwind(ref ctx);
            }
        }
    }
}