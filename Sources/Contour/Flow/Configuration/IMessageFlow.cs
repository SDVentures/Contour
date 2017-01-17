using System.Collections.Generic;

namespace Contour.Flow.Configuration
{
    public interface IMessageFlow : IIncomingFlow, IFlowEntry, IFlowRegistryItem
    {
        string Id { get; }
    }
}