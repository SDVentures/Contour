// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRouteResolver.cs" company="">
//   
// </copyright>
// <summary>
//   The RouteResolver interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Sending
{
    /// <summary>
    /// The RouteResolver interface.
    /// </summary>
    public interface IRouteResolver
    {
        /// <summary>
        /// The resolve.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="IRoute"/>.
        /// </returns>
        IRoute Resolve(IEndpoint endpoint, MessageLabel label);
    }
}
