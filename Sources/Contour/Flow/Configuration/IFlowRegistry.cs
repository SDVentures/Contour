using System.Collections.Generic;

namespace Contour.Flow.Configuration
{
    public interface IFlowRegistry
    {
        IEnumerable<IFlowRegistryItem> Get<TOutput>();

        IEnumerable<IFlowRegistryItem> Get(string label);

        IEnumerable<IFlowRegistryItem> Get<TOutput>(string label); 
        
        void Add(IFlowRegistryItem flow);
    }
}