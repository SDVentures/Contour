namespace Contour.Configurator
{
    using System;

    /// <summary>
    /// The RequestConfiguration interface.
    /// </summary>
    public interface IRequestConfiguration
    {
        #region Public Properties

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

        #endregion
    }
}
