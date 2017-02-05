using Contour.Flow.Configuration;

namespace Contour.Flow.Transport
{
    internal class LocalFlowTransport: ILocalFlowTransport
    {
        public IMessageFlow<TOutput> CreateFlow<TOutput>()
        {
            var flow = new LocalMessageFlow<TOutput>(this);
            return flow;
        }

        public string GetTailLabel(string sourceLabel)
        {
            return sourceLabel + ".tail";
        }
    }
}