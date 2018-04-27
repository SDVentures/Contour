namespace Contour.Caching
{
    using System;

    public interface ICache
    {
        object this[IMessage key] { get; }

        bool ContainsKey(IMessage key);

        void Set(IMessage key, object value, TimeSpan ttl);
    }
}