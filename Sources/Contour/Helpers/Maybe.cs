// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Maybe.cs" company="">
//   
// </copyright>
// <summary>
//   The maybe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Contour.Helpers
{
    /// <summary>
    /// The maybe.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    [Serializable]
    [Obsolete]
    public sealed class Maybe<T> : Maybe
    {
        #region Static Fields

        /// <summary>
        /// The empty.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T));

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public Maybe(T value) : base(value)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public override bool HasValue => !Equals(this.value, default(T));

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value
        {
            get
            {
                this.AssertNotNullValue();
                return (T)this.value;
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
        protected override void AssertNotNullValue()
        {
            if (!this.HasValue)
            {
                throw new InvalidOperationException(string.Format("Maybe of {0} must have value.", typeof(T)));
            }
        }

        #endregion
    }

    public class Maybe
    {
        protected readonly object value;

        public Maybe(object value)
        {
            this.value = value;
        }

        public virtual bool HasValue => !Equals(this.value, null);

        public object Value
        {
            get
            {
                this.AssertNotNullValue();
                return this.value;
            }
        }

        protected virtual void AssertNotNullValue()
        {
            if (!this.HasValue)
            {
                throw new InvalidOperationException($"Maybe of object must have value.");
            }
        }
    }
}
