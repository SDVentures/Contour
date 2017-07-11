using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The message element.
    /// </summary>
    internal class MessageElement : ExtensibleConfigurationElement
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public string Key => (string)base["key"];

        /// <summary>
        /// Gets the label.
        /// </summary>
        [ConfigurationProperty("label", IsRequired = true)]
        public string Label => (string)base["label"];
    }
}
