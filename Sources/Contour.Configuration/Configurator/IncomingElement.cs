using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The incoming element.
    /// </summary>
    internal class IncomingElement : MessageElement
    {
        /// <summary>
        /// Gets the lifestyle.
        /// </summary>
        [ConfigurationProperty("lifestyle", DefaultValue = Configurator.Lifestyle.Normal)]
        public Lifestyle? Lifestyle => (Lifestyle?)base["lifestyle"];

        /// <summary>
        /// Gets the react.
        /// </summary>
        [ConfigurationProperty("react", IsRequired = true)]
        public string React
        {
            get
            {
                return (string)base["react"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether requires accept.
        /// </summary>
        [ConfigurationProperty("requiresAccept")]
        public bool RequiresAccept
        {
            get
            {
                return (bool)(base["requiresAccept"]);
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type")]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }

        /// <summary>
        /// Gets the validate.
        /// </summary>
        [ConfigurationProperty("validate")]
        public string Validate
        {
            get
            {
                return (string)base["validate"];
            }
        }

        /// <summary>
        /// Gets QoS for incoming messages listener
        /// </summary>
        [ConfigurationProperty("qos")]
        public QosElement Qos
        {
            get
            {
                return (QosElement)base["qos"];
            }
        }

        [ConfigurationProperty("parallelismLevel", IsRequired = false)]
        public uint? ParallelismLevel
        {
            get
            {
                return (uint?) base["parallelismLevel"];
            }
        }

        [ConfigurationProperty("connectionString", IsRequired = false)]
        public string ConnectionString
        {
            get { return (string) base["connectionString"]; }
        }

        [ConfigurationProperty("reuseConnection", IsRequired = false)]
        public bool? ReuseConnection
        {
            get { return (bool?) base["reuseConnection"]; }
        }
    }
}
