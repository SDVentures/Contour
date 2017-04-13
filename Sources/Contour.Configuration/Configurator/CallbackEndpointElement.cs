using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The callback endpoint element.
    /// </summary>
    internal class CallbackEndpointElement : ConfigurationElement
    {
        /// <summary>
        /// Gets a value indicating whether default.
        /// </summary>
        [ConfigurationProperty("default")]
        public bool Default => (bool)(base["default"]);
    }
}
