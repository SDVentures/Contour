using System;

namespace Contour.Caching
{
    public interface ICachePolicy
    {
        IKeyProvider KeyProvider { get; }

        ICacheProvider CacheProvider { get; }

        TimeSpan Period { get; }
    }
}