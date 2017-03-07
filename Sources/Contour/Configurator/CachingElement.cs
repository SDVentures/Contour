namespace Contour.Configurator
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Элемент конфигурации задающий политику кеширования.
    /// </summary>
    public class CachingElement : ConfigurationElement
    {
        /// <summary>
        /// Признак включения или отключения кеширования.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)(base["enabled"]);
            }
        }

        /// <summary>
        /// Configuration extension properties
        /// </summary>
        public IDictionary<string, string> ExtensionProperties { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization
        /// </summary>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false
        /// </returns>
        /// <param name="name">The name of the unrecognized attribute</param>
        /// <param name="value">The value of the unrecognized attribute</param>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this.ExtensionProperties[name] = value;
            return true;
        }
    }
}
