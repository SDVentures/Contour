using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// Конфигурационный элемент для установки параметров динамической маршрутизации.
    /// </summary>
    public class DynamicElement : ConfigurationElement
    {
        /// <summary>
        /// Включение динамической маршрутизации для исходящих сообщений.
        /// </summary>
        [ConfigurationProperty("outgoing", IsRequired = true)]
        public bool? Outgoing => (bool?)(base["outgoing"]);
    }
}
