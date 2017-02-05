using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    internal class TailFlow<TOutput> : ITailFlow<TOutput>, IFlowSource<TOutput>
    {
        private readonly ISourceBlock<TOutput> source;

        public string Label { get; }
        public Type Type { get; }

        public IFlowRegistry Registry { get; set; }

        public IFlowTransport Transport { get; }

        public TailFlow(ISourceBlock<TOutput> source)
        {
            this.source = source;
        }

        public IFlowEntry<TIn> OnRequest<TIn>(string id, Predicate<TOutput> correlationQuery, Action<TOutput> callback)
        {
            throw new NotImplementedException();
        }

        public ISourceBlock<TOutput> AsSource()
        {
            throw new NotImplementedException();
        }
    }
}