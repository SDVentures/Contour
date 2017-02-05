using System;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A flow item controlled by the flow registry
    /// </summary>
    public interface IFlowRegistryItem
    {
        /// <summary>
        /// Flow originating label
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Flow message type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Flow registry reference
        /// </summary>
        IFlowRegistry Registry { set; }

        /// <summary>
        /// Gets a transport used to create a flow
        /// </summary>
        IFlowTransport Transport { get; }
    }
}