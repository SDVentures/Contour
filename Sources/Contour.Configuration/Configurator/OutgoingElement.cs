using System;
using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The outgoing element.
    /// </summary>
    public class OutgoingElement : MessageElement
    {
        /// <summary>
        /// Gets the callback endpoint.
        /// </summary>
        [ConfigurationProperty("callbackEndpoint")]
        public CallbackEndpointElement CallbackEndpoint => (CallbackEndpointElement)(base["callbackEndpoint"]);

        /// <summary>
        /// Gets a value indicating whether confirm.
        /// </summary>
        [ConfigurationProperty("confirm")]
        public bool Confirm => (bool)(base["confirm"]);

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

        /// <summary>
        /// Gets the connection string
        /// </summary>
        [ConfigurationProperty("connectionString", IsRequired = false)]
        public string ConnectionString => (string)base["connectionString"];

        /// <summary>
        /// Gets a flag which specifies if an existing connection should be reused
        /// </summary>
        [ConfigurationProperty("reuseConnection", IsRequired = false)]
        public bool? ReuseConnection => (bool?)base["reuseConnection"];
    }
}
