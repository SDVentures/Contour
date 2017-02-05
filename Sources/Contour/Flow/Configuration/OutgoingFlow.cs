using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class OutgoingFlow<TInput>: IOutgoingFlow<TInput>
    {
        private readonly IDataflowBlock source;

        public OutgoingFlow(IDataflowBlock source)
        {
            this.source = source;
        }

        public string Label { get; set; }

        public IFlowTransport Transport { private get; set; }
        public ITailFlow<TInput> Respond(int capacity = 1)
        {
            throw new NotImplementedException();
        }

        public ITailFlow<TInput> Forward(string label)
        {
            throw new NotImplementedException();
        }
    }
}