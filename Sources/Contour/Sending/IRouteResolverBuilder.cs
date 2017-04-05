// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRouteResolverBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   The RouteResolverBuilder interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Sending
{
    using Contour.Topology;

    /// <summary>
    /// The RouteResolverBuilder interface.
    /// </summary>
    public interface IRouteResolverBuilder
    {
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        IEndpoint Endpoint { get; }

        /// <summary>
        /// Gets the sender.
        /// </summary>
        ISenderConfiguration Sender { get; }

        /// <summary>
        /// Gets the topology.
        /// </summary>
        ITopologyBuilder Topology { get; }
    }
}
