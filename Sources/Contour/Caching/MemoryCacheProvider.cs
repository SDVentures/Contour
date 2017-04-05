namespace Contour.Caching
{
    using System;
    using System.Runtime.Caching;

    using Contour.Helpers;
    using Contour.Helpers.CodeContracts;

    /// <summary>
    /// The memory cache provider.
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        /// <summary>
        /// The prefix.
        /// </summary>
        private const string Prefix = "ServiceBus.";
        /// <summary>
        /// The _cache.
        /// </summary>
        private readonly MemoryCache _cache;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MemoryCacheProvider"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache.
        /// </param>
        public MemoryCacheProvider(MemoryCache cache)
        {
            this._cache = cache;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MemoryCacheProvider"/>.
        /// </summary>
        public MemoryCacheProvider()
            : this(MemoryCache.Default)
        {
        }
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
        public Maybe<T> Find<T>(string key) where T : class
        {
            return (T)this._cache.Get(Prefix + key);
        }

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
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public T Get<T>(string key) where T : class
        {
            Maybe<T> value = this.Find<T>(key);
            if (!value.HasValue)
            {
                throw new InvalidOperationException("No item with key [{0}] exists is cache.".FormatEx(key));
            }

            return value.Value;
        }

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
        public void Put<T>(string key, T value, TimeSpan expirationPeriod) where T : class
        {
            Requires.NotNullOrEmpty(key, "key");
            Requires.NotNull(value, "value");

            this._cache.Add(Prefix + key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.ToLocalTime() + expirationPeriod });
        }

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
        public void Put<T>(string key, T value, DateTimeOffset expirationDate) where T : class
        {
            Requires.NotNullOrEmpty(key, "key");

            this._cache.Add(Prefix + key, value, new CacheItemPolicy { AbsoluteExpiration = expirationDate });
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public void Remove(string key)
        {
            this._cache.Remove(Prefix + key);
        }
    }
}
