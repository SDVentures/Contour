namespace Contour.Caching
{
    using System;

    using Contour.Helpers;

    /// <summary>
    /// The CacheProvider interface.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        Maybe<T> Find<T>(string key) where T : class;

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The result.
        /// </returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// The put.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expirationPeriod">
        /// The expiration period.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        void Put<T>(string key, T value, TimeSpan expirationPeriod) where T : class;

        /// <summary>
        /// The put.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expirationDate">
        /// The expiration date.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        void Put<T>(string key, T value, DateTimeOffset expirationDate) where T : class;

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        void Remove(string key);
    }
}
