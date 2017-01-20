using System;

namespace Contour.Flow.Configuration
{
    internal class InMemoryFlowLabelProvider : IFlowLabelProvider
    {
        public string GetNew()
        {
            return Guid.NewGuid().ToString();
        }
    }
}