using System;

namespace Contour.Flow.Configuration
{
    class CachePolicy : ICachePolicy
    {
        public CachePolicy(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }
    }
}