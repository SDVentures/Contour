// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Exchange.cs" company="">
//   
// </copyright>
// <summary>
//   The exchange.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Topology
{
    using Contour.Sending;
    using Contour.Topology;

    using global::RabbitMQ.Client;

    /// <summary>
    /// The exchange.
    /// </summary>
    public class Exchange : ITopologyEntity, IRouteResolver
    {
        private readonly IDictionary<string, object> arguments;

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Exchange"/>.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        internal Exchange(string name)
        {
            this.Name = name;
            this.Type = ExchangeType.Direct;
            this.Durable = false;
            this.AutoDelete = false;
            this.arguments = new Dictionary<string, object>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether auto delete.
        /// </summary>
        public bool AutoDelete { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether durable.
        /// </summary>
        public bool Durable { get; internal set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public string Type { get; internal set; }

        public IDictionary<string, object> Arguments => this.arguments;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The named.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="ExchangeBuilder"/>.
        /// </returns>
        public static ExchangeBuilder Named(string name)
        {
            return new ExchangeBuilder(name);
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
            return new RabbitRoute(this.Name);
        }

        #endregion
    }
}
