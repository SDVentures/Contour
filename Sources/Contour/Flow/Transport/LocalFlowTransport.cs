using Contour.Flow.Configuration;
using Contour.Flow.Execution;

namespace Contour.Flow.Transport
{
    internal class LocalFlowTransport: ILocalFlowTransport
    {
        public IMessageFlow<TSource, TSource> CreateFlow<TSource>()
        {
            var flow = new LocalMessageFlow<TSource>(this);
            return flow;
        }
    }
}