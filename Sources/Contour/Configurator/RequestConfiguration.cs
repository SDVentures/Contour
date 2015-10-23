namespace Contour.Configurator
{
    using System;

    /// <summary>
    /// The request configuration.
    /// </summary>
    internal class RequestConfiguration : IRequestConfiguration
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RequestConfiguration"/>.
        /// </summary>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <param name="persist">
        /// The persist.
        /// </param>
        /// <param name="ttl">
        /// The ttl.
        /// </param>
        public RequestConfiguration(TimeSpan? timeout, bool persist, TimeSpan? ttl)
        {
            this.Timeout = timeout;
            this.Persistently = persist;
            this.Ttl = ttl;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether persistently.
        /// </summary>
        public bool Persistently { get; private set; }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        public TimeSpan? Timeout { get; private set; }

        /// <summary>
        /// Gets the ttl.
        /// </summary>
        public TimeSpan? Ttl { get; private set; }

        #endregion
    }
}
