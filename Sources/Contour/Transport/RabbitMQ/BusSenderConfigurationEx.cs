namespace Contour.Transport.RabbitMQ
{
    using Sending;
    
    /// <summary>
    /// Extension methods for sender configurator
    /// </summary>
    public static class BusSenderConfigurationEx
    {
        /// <summary>
        /// Configures sender to use a connection string
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <returns>
        /// The <see cref="ISenderConfigurator"/>.
        /// </returns>
        public static ISenderConfigurator WithConnectionString(this ISenderConfigurator builder, string connectionString)
        {
            ((RabbitSenderOptions)((SenderConfiguration)builder).Options).ConnectionString = connectionString;
            return builder;
        }

        /// <summary>
        /// Configures sender to reuse a connection.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="reuse">
        /// The reuse.
        /// </param>
        /// <returns>
        /// The <see cref="ISenderConfigurator"/>.
        /// </returns>
        public static ISenderConfigurator ReuseConnection(this ISenderConfigurator builder, bool reuse = true)
        {
            ((RabbitSenderOptions)((SenderConfiguration)builder).Options).ReuseConnection = reuse;
            return builder;
        }
    }
}