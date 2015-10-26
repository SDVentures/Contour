namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The digest element.
    /// </summary>
    public class DigestElement : ConfigurationElement
    {
        #region Public Properties

        /// <summary>
        /// Gets the distinct.
        /// </summary>
        [ConfigurationProperty("distinct", IsRequired = false)]
        public string Distinct
        {
            get
            {
                return (string)base["distinct"];
            }
        }

        /// <summary>
        /// Gets the flush key.
        /// </summary>
        [ConfigurationProperty("trigger-flush-key", IsRequired = true)]
        public string FlushKey
        {
            get
            {
                return (string)base["trigger-flush-key"];
            }
        }

        /// <summary>
        /// Gets the group by.
        /// </summary>
        [ConfigurationProperty("group-by", IsKey = true, IsRequired = true)]
        public string GroupBy
        {
            get
            {
                return (string)base["group-by"];
            }
        }

        /// <summary>
        /// Gets the output key.
        /// </summary>
        [ConfigurationProperty("output-key", IsRequired = true)]
        public string OutputKey
        {
            get
            {
                return (string)base["output-key"];
            }
        }

        /// <summary>
        /// Gets the store key.
        /// </summary>
        [ConfigurationProperty("trigger-store-key", IsRequired = true)]
        public string StoreKey
        {
            get
            {
                return (string)base["trigger-store-key"];
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }

        #endregion
    }
}
