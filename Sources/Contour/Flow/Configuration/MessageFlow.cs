using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Blocks;

namespace Contour.Flow.Configuration
{
    public class MessageFlow : IMessageFlow
    {
        private IDataflowBlock block;

        public MessageFlow()
        {
        }

        public MessageFlow(IDataflowBlock block)
        {
            this.block = block;
        }

        public Task Completion => block.Completion;

        public IActionableFlow<TInput> On<TInput>(string label, int capacity)
        {
            if (block == null)
                block = new SourceMessageBlock<TInput>(label, capacity);

            var flow = new ActionableFlow<TInput>((ISourceBlock<TInput>) block); //todo validate casting
            return flow;
        }

        IMessageFlow IMessageFlow.Also()
        {
            throw new NotImplementedException();
        }
    }
}