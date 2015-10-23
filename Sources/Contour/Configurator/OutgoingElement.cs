namespace Contour.Configurator
{
    using System;
    using System.Configuration;

    /// <summary>
    /// The outgoing element.
    /// </summary>
    internal class OutgoingElement : MessageElement
    {
        #region Public Properties

        /// <summary>
        /// Gets the callback endpoint.
        /// </summary>
        [ConfigurationProperty("callbackEndpoint")]
        public CallbackEndpointElement CallbackEndpoint
        {
            get
            {
                return (CallbackEndpointElement)(base["callbackEndpoint"]);
            }
        }

        /// <summary>
        /// Gets a value indicating whether confirm.
        /// </summary>
        [ConfigurationProperty("confirm")]
        public bool Confirm
        {
            get
            {
                return (bool)(base["confirm"]);
            }
        }

        /// <summary>
        /// Gets a value indicating whether persist.
        /// </summary>
        [ConfigurationProperty("persist")]
        public bool Persist
        {
            get
            {
                return (bool)base["persist"];
            }
        }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        [ConfigurationProperty("timeout", DefaultValue = "00:00:30")]
        public TimeSpan? Timeout
        {
            get
            {
                return (TimeSpan?)base["timeout"];
            }
        }

        /// <summary>
        /// Gets the ttl.
        /// </summary>
        [ConfigurationProperty("ttl")]
        public TimeSpan? Ttl
        {
            get
            {
                return (TimeSpan?)base["ttl"];
            }
        }

        #endregion
    }
}
