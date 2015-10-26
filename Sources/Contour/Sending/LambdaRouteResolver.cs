// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LambdaRouteResolver.cs" company="">
//   
// </copyright>
// <summary>
//   The lambda route resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Sending
{
    using System;

    /// <summary>
    /// The lambda route resolver.
    /// </summary>
    public class LambdaRouteResolver : IRouteResolver
    {
        #region Fields

        /// <summary>
        /// The _resolver func.
        /// </summary>
        private readonly Func<IEndpoint, MessageLabel, IRoute> _resolverFunc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaRouteResolver"/>.
        /// </summary>
        /// <param name="resolverFunc">
        /// The resolver func.
        /// </param>
        public LambdaRouteResolver(Func<IEndpoint, MessageLabel, IRoute> resolverFunc)
        {
            this._resolverFunc = resolverFunc;
        }

        #endregion

        #region Public Methods and Operators

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
            return this._resolverFunc(endpoint, label);
        }

        #endregion
    }
}
