namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// Элемент конфигурации задающий политику кеширования.
    /// </summary>
    internal class CachingElement : ConfigurationElement
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
    }
}
