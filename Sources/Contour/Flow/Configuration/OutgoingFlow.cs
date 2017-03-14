using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    internal class OutgoingFlow<TSource, TOutput>: IOutgoingFlow<TSource, TOutput>
    {
        private readonly IDataflowBlock source;
        
        public IFlowRegistry Registry { private get; set; }
        public string Label { private get; set; }

        public OutgoingFlow(IDataflowBlock source)
        {
            this.source = source;
        }
        
        public IRequestResponseFlow<TSource, TOutput> Forward(string label)
        {
            throw new NotImplementedException();
        }

        public IRequestResponseFlow<TSource, TOutput> Respond()
        {
            throw new NotImplementedException();
        }
    }
}