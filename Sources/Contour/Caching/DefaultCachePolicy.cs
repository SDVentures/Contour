using System;

namespace Contour.Caching
{
    internal class DefaultCachePolicy : ICachePolicy
    {
        public IKeyProvider KeyProvider { get; }

        public ICacheProvider CacheProvider { get; }

        public TimeSpan Period { get; }

        public DefaultCachePolicy(TimeSpan period)
        {
            KeyProvider = new HashKeyProvider();
            CacheProvider = new MemoryCacheProvider();
            Period = period;
        }
    }
}