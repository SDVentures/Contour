using System;

namespace Contour.Flow.Configuration
{
    public interface IFlowFactory
    {
        IMessageFlow<TOutput> Create<TOutput>(string transportName);
    }
}