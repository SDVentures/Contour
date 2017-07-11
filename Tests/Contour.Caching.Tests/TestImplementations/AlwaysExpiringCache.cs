namespace Contour.Caching.Tests.TestImplementations
{
    using System;

    public class AlwaysExpiringCache : ICache
    {
        public object this[IMessage key] => null;

        public bool ContainsKey(IMessage key)
        {
            return false;
        }

        public void Set(IMessage key, object value, TimeSpan ttl)
        {
        }
    }
}