namespace Contour.Configuration
{
    using System;

    using Contour.Helpers;

    /// <summary>
    /// The bus options.
    /// </summary>
    public abstract class BusOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusOptions"/> class. 
        /// </summary>
        protected BusOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusOptions"/> class. 
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        protected BusOptions(BusOptions parent)
        {
            this.Parent = parent;
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public BusOptions Parent { get; private set; }

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

        /// <summary>
        /// The pick.
        /// </summary>
        /// <param name="getValue">
        /// The get value func.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        protected Maybe<T> Pick<P,T>(Func<P, Maybe<T>> getValue) where P: BusOptions
        {
            if (this is P)
            {
                var value = getValue(this as P);
                if (value != null && value.HasValue)
                {
                    return value;
                }

                if (this.Parent != null)
                {
                    return this.Parent.Pick(getValue);
                }
            }

            return Maybe<T>.Empty;
        }
    }
}
