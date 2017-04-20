using System;
using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The endpoint element.
    /// </summary>
    public class EndpointElement : ExtensibleConfigurationElement
    {
        /// <summary>
        /// Настройки QoS для конечной точки.
        /// </summary>
        [ConfigurationProperty("qos")]
        public QosElement Qos => (QosElement)base["qos"];

        /// <summary>
        /// Настройки динамической маршрутизации.
        /// </summary>
        [ConfigurationProperty("dynamic")]
        public DynamicElement Dynamic => (DynamicElement)base["dynamic"];

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get
            {
                return (string)this["connectionString"];
            }

            set
            {
                this["connectionString"] = value;
            }
        }

        [ConfigurationProperty("reuseConnection", DefaultValue = true, IsRequired = false)]
        public bool? ReuseConnection
        {
            get { return (bool?)this["reuseConnection"]; }
            set { this["reuseConnection"] = value; }
        }

        /// <summary>
        /// Количество одновременных обработчиков сообщений из очередей конечной точки, включая очереди ответных сообщений.
        /// </summary>
        [ConfigurationProperty("parallelismLevel", IsRequired = false)]
        public uint? ParallelismLevel
        {
            get
            {
                return (uint?)this["parallelismLevel"];
            }

            set
            {
                this["parallelismLevel"] = value;
            }
        }

        /// <summary>
        /// Время хранения сообщений в Fault очереди.
        /// </summary>
        [ConfigurationProperty("faultQueueTtl", IsRequired = false)]
        public TimeSpan? FaultQueueTtl => (TimeSpan?)this["faultQueueTtl"];

        [ConfigurationProperty("faultQueueLimit", IsRequired = false)]
        public int? FaultQueueLimit => (int?)this["faultQueueLimit"];

        [ConfigurationProperty("connectionStringProvider", IsRequired = false, DefaultValue = null)]
        public string ConnectionStringProvider => (string)this["connectionStringProvider"];

        /// <summary>
        /// Gets the incoming.
        /// </summary>
        [ConfigurationProperty("incoming")]
        public IncomingCollection Incoming => (IncomingCollection)base["incoming"];

        /// <summary>
        /// Gets or sets the lifecycle handler.
        /// </summary>
        [ConfigurationProperty("lifecycleHandler")]
        public string LifecycleHandler
        {
            get
            {
                return (string)this["lifecycleHandler"];
            }

            set
            {
                this["lifecycleHandler"] = value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name => (string)base["name"];

        /// <summary>
        /// Gets the outgoing.
        /// </summary>
        [ConfigurationProperty("outgoing")]
        public OutgoingCollection Outgoing => (OutgoingCollection)base["outgoing"];

        /// <summary>
        /// Gets the validators.
        /// </summary>
        [ConfigurationProperty("validators")]
        public ValidatorCollection Validators => (ValidatorCollection)base["validators"];
    }
}
