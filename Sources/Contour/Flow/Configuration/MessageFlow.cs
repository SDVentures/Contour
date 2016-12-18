using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    public class MessageFlow: IMessageFlow
    {
        public IActionableFlow On<TInput>(string label, int capacity = 1)
        {
            var block = new SourceBlock<TInput>(label, capacity);
            var actionable = new ActionableFlow(block);
            return actionable;
        }

        public IMessageFlow Also()
        {
            throw new NotImplementedException();
        }
    }
}