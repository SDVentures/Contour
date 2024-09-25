// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopologyBuilderEx.cs" company="">
//   
// </copyright>
// <summary>
//   The topology builder ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Topology
{
    using Contour.Topology;

    /// <summary>
    /// The topology builder ex.
    /// </summary>
    public static class TopologyBuilderEx
    {
        #region Public Methods and Operators

        /// <summary>
        /// The bind.
        /// </summary>
        /// <param name="topology">
        /// The topology.
        /// </param>
        /// <param name="exchange">
        /// The exchange.
        /// </param>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        public static void Bind(this ITopologyBuilder topology, Exchange exchange, Queue queue, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            ((TopologyBuilder)topology).Bind(exchange, queue, routingKey, arguments);
        }

        /// <summary>
        /// The bind.
        /// </summary>
        /// <param name="topology">
        /// The topology.
        /// </param>
        /// <param name="exchangeName">
        /// The exchange name.
        /// </param>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="routingKey">
        /// The routing key.
        /// </param>
        public static void Bind(this ITopologyBuilder topology, string exchangeName, Queue queue, string routingKey = "")
        {
            Bind(
                topology, 
                Exchange.Named(exchangeName).
                    Instance, 
                queue, 
                routingKey);
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="topology">
        /// The topology.
        /// </param>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <returns>
        /// The <see cref="Exchange"/>.
        /// </returns>
        public static Exchange Declare(this ITopologyBuilder topology, ExchangeBuilder builder)
        {
            return ((TopologyBuilder)topology).Declare(builder);
        }

        /// <summary>
        /// The declare.
        /// </summary>
        /// <param name="topology">
        /// The topology.
        /// </param>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <returns>
        /// The <see cref="Queue"/>.
        /// </returns>
        public static Queue Declare(this ITopologyBuilder topology, QueueBuilder builder)
        {
            return ((TopologyBuilder)topology).Declare(builder);
        }

        #endregion
    }
}
