using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class OutgoingFlow<TOutput>: IOutgoingFlow<TOutput>
    {
        private readonly IDataflowBlock source;

        public OutgoingFlow(IDataflowBlock source)
        {
            this.source = source;
        }

        public string Label { get; set; }

        public IFlowTransport Transport { private get; set; }

        public IResponseFlow<TOutput> Respond(int capacity = 1)
        {
            throw new NotImplementedException();
        }

        public IResponseFlow<TOutput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}