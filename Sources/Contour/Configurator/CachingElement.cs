using Contour.Configurator.Configuration;

namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// Элемент конфигурации задающий политику кеширования.
    /// </summary>
    internal class CachingElement : ConfigurationElement, ICaching
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

        bool ICaching.Enabled => this.Enabled;
    }
}
