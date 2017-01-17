using System.Collections.Generic;

namespace Contour.Flow.Configuration
{
    public interface IFlowRegistry
    {
        IEnumerable<IFlowTarget> Get<TOutput>();

        void Add(IFlowTarget flow);
    }
}