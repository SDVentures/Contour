using System.Configuration;

namespace Contour.Configurator
{
    /// <summary>
    /// Конфигурационный элемент для установки параметров <c>QoS</c> (<a href="http://www.rabbitmq.com/blog/2012/05/11/some-queuing-theory-throughput-latency-and-bandwidth/"><c>QoS</c></a>).
    /// </summary>
    public class QosElement : ConfigurationElement
    {
        /// <summary>
        /// Количество считываемых сообщений из очереди.
        /// </summary>
        [ConfigurationProperty("prefetchCount", IsRequired = true)]
        public ushort? PrefetchCount
        {
            get
            {
                return (ushort?)base["prefetchCount"];
            }
        }
    }
}
