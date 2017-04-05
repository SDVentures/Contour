// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticRouter.cs" company="">
//   
// </copyright>
// <summary>
//   The static route resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Transport.RabbitMQ
{
    using Contour.Sending;
    using Contour.Transport.RabbitMQ.Topology;

    /// <summary>
    /// The static route resolver.
    /// </summary>
    public class StaticRouteResolver : IRouteResolver
    {
        /// <summary>
        /// The _route.
        /// </summary>
        private readonly IRoute _route;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StaticRouteResolver"/>.
        /// </summary>
        /// <param name="route">
        /// The route.
        /// </param>
        public StaticRouteResolver(IRoute route)
        {
            this._route = route;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StaticRouteResolver"/>.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        public StaticRouteResolver(string exchange, string routingKey = "")
        {
            this._route = new RabbitRoute(exchange, routingKey);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StaticRouteResolver"/>.
        /// </summary>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        public StaticRouteResolver(Exchange exchange, string routingKey = "")
            : this(exchange.Name, routingKey)
        {
        }
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
        public IRoute Resolve(IEndpoint endpoint, MessageLabel label)
        {
            return this._route;
        }
    }
}
