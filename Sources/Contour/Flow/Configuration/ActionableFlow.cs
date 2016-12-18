using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class ActionableFlow : IActionableFlow
    {
        private IDataflowBlock block;

        public ActionableFlow(IDataflowBlock block)
        {
            this.block = block;
        }

        public ICachebleFlow Respond<TInput, TOutput>(Func<TInput, TOutput> responder)
        {
            throw new NotImplementedException();
        }

        public ICachebleFlow Respond()
        {
            throw new NotImplementedException();
        }

        public IMessageFlow Forward(string label)
        {
            throw new NotImplementedException();
        }

        public IActionableFlow Act<TInput, TOutput>(Func<TInput, TOutput> act, int scale = 1)
        {
            if (block is ISourceBlock<TInput>)
            {
                var sourceBlock = (ISourceBlock<TInput>) block;
                var actBlock = new TransformBlock<TInput, TOutput>(act,
                    new ExecutionDataflowBlockOptions() {MaxDegreeOfParallelism = scale});

                sourceBlock.LinkTo(actBlock);
                var actFlow = new ActionableFlow(actBlock);
                return actFlow;
            }
            else
                throw new InvalidFlowTypeException();
        }
    }
}