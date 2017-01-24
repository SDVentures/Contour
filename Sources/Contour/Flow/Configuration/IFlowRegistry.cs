using System.Collections.Generic;

namespace Contour.Flow.Configuration
{
    public interface IFlowRegistry
    {
        IEnumerable<IFlowTarget> Get<TOutput>();

        IEnumerable<IFlowTarget> Get(string label);

        IEnumerable<IFlowTarget> Get<TOutput>(string label); 
        
        void Add(IFlowTarget flow);
    }
}