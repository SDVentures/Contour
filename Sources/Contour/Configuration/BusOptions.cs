namespace Contour.Configuration
{
    using System;

    using Contour.Helpers;

    /// <summary>
    /// The bus options.
    /// </summary>
    public abstract class BusOptions
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusOptions"/>.
        /// </summary>
        protected BusOptions()
        {
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusOptions"/>.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        protected BusOptions(BusOptions parent)
        {
            this.Parent = parent;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public BusOptions Parent { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The pick.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        public static Maybe<T> Pick<T>(Maybe<T> first, Maybe<T> second)
        {
            if (first != null && first.HasValue)
            {
                return first;
            }

            return second;
        }

        /// <summary>
        /// The derive.
        /// </summary>
        /// <returns>
        /// The <see cref="BusOptions"/>.
        /// </returns>
        public abstract BusOptions Derive();

        #endregion

        #region Methods

        /// <summary>
        /// The pick.
        /// </summary>
        /// <param name="getValueFunc">
        /// The get value func.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        protected Maybe<T> Pick<T>(Func<BusOptions, Maybe<T>> getValueFunc)
        {
            Maybe<T> value = getValueFunc(this);
            if (value != null && value.HasValue)
            {
                return value;
            }

            if (this.Parent != null)
            {
                return getValueFunc(this.Parent);
            }

            return Maybe<T>.Empty;
        }

        #endregion
    }
}
