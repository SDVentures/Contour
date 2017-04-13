namespace Contour.Configurator
{
    using System;

    /// <summary>
    /// The RequestConfiguration interface.
    /// </summary>
    public interface IRequestConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether persistently.
        /// </summary>
        bool Persistently { get; }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Gets the ttl.
        /// </summary>
        TimeSpan? Ttl { get; }
    }
}
