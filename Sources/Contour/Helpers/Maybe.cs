// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Maybe.cs" company="">
//   
// </copyright>
// <summary>
//   The maybe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Helpers
{
    using System;

    /// <summary>
    /// The maybe.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    [Serializable]
    public sealed class Maybe<T>
    {
        #region Static Fields

        /// <summary>
        /// The empty.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T));

        #endregion

        #region Fields

        /// <summary>
        /// The value.
        /// </summary>
        private readonly T value;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public Maybe(T value)
        {
            this.value = value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public bool HasValue
        {
            get
            {
                return !Equals(this.value, default(T));
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value
        {
            get
            {
                this.AssertNotNullValue();
                return this.value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="maybe">
        /// The maybe.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator T(Maybe<T> maybe)
        {
            maybe.AssertNotNullValue();
            return maybe.Value;
        }

        /// <summary>
        /// The op_ implicit.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// </returns>
        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        /// <summary>
        /// The op_ implicit.
        /// </summary>
        /// <param name="maybe">
        /// The maybe.
        /// </param>
        /// <returns>
        /// </returns>
        public static implicit operator bool(Maybe<T> maybe)
        {
            return maybe != null && maybe.HasValue;
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.HasValue ? this.value.ToString() : string.Format("Empty Maybe of {0}.", typeof(T));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The assert not null value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void AssertNotNullValue()
        {
            if (!this.HasValue)
            {
                throw new InvalidOperationException(string.Format("Maybe of {0} must have value.", typeof(T)));
            }
        }

        #endregion
    }
}
