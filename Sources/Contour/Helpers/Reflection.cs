namespace Contour.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The reflection.
    /// </summary>
    internal static class Reflection
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get generic type parameter of.
        /// </summary>
        /// <param name="openType">
        /// The open type.
        /// </param>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="IList{T}"/>.
        /// </returns>
        public static IList<Type> GetGenericTypeParameterOf(Type openType, object obj)
        {
            return (from iType in obj.GetType().
                        GetInterfaces()
                    where iType.IsGenericType && iType.GetGenericTypeDefinition() == openType
                    select iType.GetGenericArguments()[0]).ToList();
        }

        #endregion
    }
}
