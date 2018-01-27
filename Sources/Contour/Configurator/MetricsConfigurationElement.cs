using System.Configuration;

namespace Contour.Configurator
{
    internal class MetricsConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("collector")]
        public string Collector => (string)this["collector"];
    }
}