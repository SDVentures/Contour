namespace Contour.Caching
{
    using System;

    public class CacheConfiguration
    {
        public CacheConfiguration(bool? enabled, TimeSpan? ttl, ICache cache)
        {
            this.Enabled = enabled;
            this.Ttl = ttl;
            this.Cache = cache;
        }

        public bool? Enabled { get; }

        public TimeSpan? Ttl { get; }

        public ICache Cache { get; }
    }
}