namespace Contour.Configuration.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The message element.
    /// </summary>
    public class MessageElement : ExtensibleConfigurationElement
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
