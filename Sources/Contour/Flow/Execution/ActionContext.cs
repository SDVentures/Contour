using System;

namespace Contour.Flow.Execution
{
    public class ActionContext<TInput, TOutput>: Context
    {
        public TInput Arg { get; set; }

        public TOutput Result { get; set; }

        public Exception Error { get; set; }
        
        public override object GetArg()
        {
            return Arg;
        }

        public Context Unwind()
        {
            Context ctx = this;
            unwind(ref ctx);

            return ctx;
        }

        private void unwind(ref Context ctx)
        {
            if (ctx.GetArg() is Context)
            {
                var context = Arg as Context;
                unwind(ref context);
            }
        }
    }

    public class ActionContext<TInput>: Context
    {
        public TInput Arg { get; set; }

        public Exception Error { get; set; }

        public override object GetArg()
        {
            return Arg;
        }
    }

    public abstract class Context
    {
        public abstract object GetArg();
    }
}