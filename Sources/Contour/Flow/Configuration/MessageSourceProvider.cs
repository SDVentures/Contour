using System;
using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    public class MessageSourceProvider<TOutput> : ISourceProvider<TOutput>
    {
        public ISourceBlock<TOutput> Source()
        {
            throw new NotImplementedException();
        }
    }
}