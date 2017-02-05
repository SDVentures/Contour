using System;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an outgoing message flow
    /// </summary>
    public interface IOutgoingFlow<TOutput>
    {
        IFlowTransport Transport { set; }

        /// <summary>
        /// Sends any flow handling results to the outgoing flow if provided
        /// </summary>
        IResponseFlow<TOutput> Respond(int capacity = 1);

        /// <summary>
        /// Forwards any flow handling results to the flow identified by <paramref name="label"/>
        /// </summary>
        /// <param name="label"></param>
        IResponseFlow<TOutput> Forward(string label);
    }
}