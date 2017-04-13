using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// Элемент конфигурации задающий политику кеширования.
    /// </summary>
    internal class CachingElement : ConfigurationElement
    {
        /// <summary>
        /// Признак включения или отключения кеширования.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled => (bool)(base["enabled"]);
    }
}
