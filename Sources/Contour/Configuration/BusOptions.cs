using System;

using Contour.Helpers;

namespace Contour.Configuration
{
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
        [Obsolete("Use nullable-typed version instead")]
        public static Maybe<T> Pick<T>(Maybe<T> first, Maybe<T> second)
        {
            if (first != null && first.HasValue)
            {
                return first;
            }

            return second;
        }

        /// <summary>
        /// Picks the first value if it is not null and the second otherwise
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <typeparam name="T">The type of the option
        /// </typeparam>
        /// <returns>
        /// The value of the option
        /// </returns>
        public static T? Pick<T>(T? first, T? second) where T : struct
        {
            if (first != null)
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
        [Obsolete("Use nullable-typed version instead")]
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

        /// <summary>
        /// Picks the value of an option. If no value is provided uses the parent object to extract the value.
        /// </summary>
        /// <param name="getValue">
        /// The get value.
        /// </param>
        /// <typeparam name="TP">The type of the object used to extract the value
        /// </typeparam>
        /// <typeparam name="TV">The type of the option which should be a not-nullable type
        /// </typeparam>
        /// <returns>
        /// The value of the option
        /// </returns>
        protected TV? Pick<TP, TV>(Func<TP, TV?> getValue) where TP : BusOptions where TV : struct
        {
            if (this is TP)
            {
                var value = getValue(this as TP);
                if (value != null)
                {
                    return value;
                }

                return this.Parent?.Pick(getValue);
            }

            return null;
        }

        /// <summary>
        /// Picks the value of an option. If no value is provided uses the parent object to extract the value.
        /// </summary>
        /// <param name="getValue">
        /// The get value.
        /// </param>
        /// <typeparam name="TP">The type of the object used to extract the value
        /// </typeparam>
        /// <typeparam name="TV">The type of the option
        /// </typeparam>
        /// <returns>
        /// The value of the option
        /// </returns>
        protected TV Pick<TP, TV>(Func<TP, TV> getValue) where TP : BusOptions
        {
            if (this is TP)
            {
                var value = getValue(this as TP);
                if (value != null)
                {
                    return value;
                }

                if (this.Parent != null)
                {
                    return this.Parent.Pick(getValue);
                }
            }

            return default(TV);
        }
    }
}
