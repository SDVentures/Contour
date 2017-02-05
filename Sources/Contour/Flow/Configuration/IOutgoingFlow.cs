using System;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an outgoing message flow
    /// </summary>
    public interface IOutgoingFlow<TInput>
    {
        string Label { set; }

        IFlowTransport Transport { set; }

        /// <summary>
        /// Sends any flow handling results to the outgoing flow if provided
        /// </summary>
        ITailFlow<TInput> Respond(int capacity = 1);

        /// <summary>
        /// Forwards any flow handling results to the flow identified by <paramref name="label"/>
        /// </summary>
        /// <param name="label"></param>
        ITailFlow<TInput> Forward(string label);
    }
}