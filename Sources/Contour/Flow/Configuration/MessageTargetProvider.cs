using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class MessageTargetProvider<TInput> : ITargetProvider<TInput>
    {
        public ITargetBlock<TInput> Target()
        {
            throw new NotImplementedException();
        }
    }
}