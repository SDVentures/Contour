// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RouteResolverBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   The route resolver builder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Sending
{
    using Contour.Topology;

    /// <summary>
    /// The route resolver builder.
    /// </summary>
    public class RouteResolverBuilder : IRouteResolverBuilder
    {
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RouteResolverBuilder"/>.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="topology">
        /// The topology.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        public RouteResolverBuilder(IEndpoint endpoint, ITopologyBuilder topology, ISenderConfiguration sender)
        {
            this.Endpoint = endpoint;
            this.Topology = topology;
            this.Sender = sender;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public IEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Gets the sender.
        /// </summary>
        public ISenderConfiguration Sender { get; private set; }

        /// <summary>
        /// Gets the topology.
        /// </summary>
        public ITopologyBuilder Topology { get; private set; }

        #endregion
    }
}
