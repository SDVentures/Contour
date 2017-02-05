using System.Collections.Generic;

namespace Contour.Flow.Configuration
{
    public interface IFlowRegistry
    {
        IEnumerable<IFlowRegistryItem> GetAll<TOutput>();

        IFlowRegistryItem Get(string label);
        
        void Add(IFlowRegistryItem flow);
    }
}