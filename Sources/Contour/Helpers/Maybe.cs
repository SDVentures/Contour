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
        /// <summary>
        /// The empty.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T));

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public Maybe(T value) : base(value)
        {
        }

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public override bool HasValue => !Equals(this.value, default(T));

        /// <summary>
        /// Gets the value.
        /// </summary>
        public new T Value
        {
            get
            {
                this.AssertNotNullValue();
                return (T)this.value;
            }
        }

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
    }

    public class Maybe
    {
        protected readonly object value;

        public Maybe(object value)
        {
            var maybe = value as Maybe;

            if (maybe == null)
            {
                this.value = value;
                return;
            }

            if (!maybe.HasValue)
            {
                this.value = null;
                return;
            }

            this.value = maybe.Value;
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
