namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The incoming element.
    /// </summary>
    internal class IncomingElement : MessageElement
    {
        #region Public Properties

        /// <summary>
        /// Gets the lifestyle.
        /// </summary>
        [ConfigurationProperty("lifestyle", DefaultValue = Configurator.Lifestyle.Normal)]
        public Lifestyle? Lifestyle
        {
            get
            {
                return (Lifestyle?)base["lifestyle"];
            }
        }

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

        #endregion
    }
}
