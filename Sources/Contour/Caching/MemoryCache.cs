namespace Contour.Caching
{
    using System;
    using System.Runtime.Caching;

    public class MemoryCache : ICache
    {
        private readonly ObjectCache innerCache;

        private readonly IHasher hasher;

        public MemoryCache(ObjectCache innerCache, IHasher hasher)
        {
            this.innerCache = innerCache;
            this.hasher = hasher;
        }

        public MemoryCache(IHasher hasher)
            : this(System.Runtime.Caching.MemoryCache.Default, hasher)
        {
        }

        public object this[IMessage key]
        {
            get
            {
                var hash = this.hasher.GetHash(key);

                return this.innerCache[hash];
            }
        }

        public bool ContainsKey(IMessage key)
        {
            var hash = this.hasher.GetHash(key);

            return this.innerCache.Contains(hash);
        }

        public void Set(IMessage key, object value, TimeSpan ttl)
        {
            var hash = this.hasher.GetHash(key);

            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.ToLocalTime() + ttl };

            this.innerCache.Set(hash, value, policy);
        }
    }
}