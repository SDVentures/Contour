using System;

namespace Contour.Flow.Configuration
{
    public interface ICachePolicy
    {
        TimeSpan Duration { get; }
    }
}