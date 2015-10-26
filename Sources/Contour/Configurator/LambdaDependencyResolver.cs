namespace Contour.Configurator
{
    using System;

    /// <summary>
    /// The dependency resolver func.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    /// <param name="type">
    /// The type.
    /// </param>
    public delegate object DependencyResolverFunc(string name, Type type);

    /// <summary>
    /// The lambda dependency resolver.
    /// </summary>
    internal class LambdaDependencyResolver : IDependencyResolver
    {
        #region Fields

        /// <summary>
        /// The _resolver.
        /// </summary>
        private readonly DependencyResolverFunc _resolver;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaDependencyResolver"/>.
        /// </summary>
        /// <param name="resolver">
        /// The resolver.
        /// </param>
        public LambdaDependencyResolver(DependencyResolverFunc resolver)
        {
            this._resolver = resolver;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The resolve.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Resolve(string name, Type type)
        {
            return this._resolver(name, type);
        }

        #endregion
    }
}
