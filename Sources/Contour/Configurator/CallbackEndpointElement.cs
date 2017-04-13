namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The callback endpoint element.
    /// </summary>
    public class CallbackEndpointElement : ConfigurationElement
    {
        /// <summary>
        /// Gets a value indicating whether default.
        /// </summary>
        [ConfigurationProperty("default")]
        public bool Default
        {
            get
            {
                return (bool)(base["default"]);
            }
        }
    }
}
