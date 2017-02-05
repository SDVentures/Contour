using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class ResponseFlow<TOutput> : IResponseFlow<TOutput>
    {
        private readonly ISourceBlock<TOutput> source;

        public string Label { get; }

        public Type Type { get; }

        public IFlowRegistry Registry { get; set; }

        public IFlowTransport Transport { get; }

        public ResponseFlow(ISourceBlock<TOutput> source)
        {
            this.source = source;
        }

        public IFlowEntry<TInput> OnResponse<TInput>(Action<TOutput> callback)
        {
            var requestFlow = new RequestFlow<TInput, TOutput>(source, callback);
            return requestFlow;
        }
    }
}