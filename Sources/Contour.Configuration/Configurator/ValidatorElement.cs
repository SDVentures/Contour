using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The validator element.
    /// </summary>
    public class ValidatorElement : ConfigurationElement
    {
        /// <summary>
        /// Gets a value indicating whether group.
        /// </summary>
        [ConfigurationProperty("group", IsRequired = false)]
        public bool Group => (bool)(base["group"]);

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name => (string)(base["name"]);
    }
}
