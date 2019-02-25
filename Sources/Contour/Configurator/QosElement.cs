using System.Configuration;

using Contour.Configurator.Configuration;

namespace Contour.Configurator
{
    /// <summary>
    /// Конфигурационный элемент для установки параметров <c>QoS</c> (<a href="http://www.rabbitmq.com/blog/2012/05/11/some-queuing-theory-throughput-latency-and-bandwidth/"><c>QoS</c></a>).
    /// </summary>
    public class QosElement : ConfigurationElement, IQos
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

        /// <summary>
        /// Количество сообщений, которые должен обработать получатель, прежде чем получит новую порцию данных.
        /// </summary>
        [ConfigurationProperty("prefetchSize", IsRequired = false)]
        public uint? PrefetchSize
        {
            get
            {
                return (uint?)base["prefetchSize"];
            }
        }
    }
}
