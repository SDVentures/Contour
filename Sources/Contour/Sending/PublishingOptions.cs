namespace Contour.Sending
{
    using System;

    using Contour.Helpers;

    /// <summary>
    /// The publishing options.
    /// </summary>
    public class PublishingOptions
    {
        /// <summary>
        /// Gets or sets the persistently.
        /// </summary>
        public Maybe<bool> Persistently { get; set; }

        /// <summary>
        /// Gets or sets the ttl.
        /// </summary>
        public Maybe<TimeSpan?> Ttl { get; set; }
    }
}
