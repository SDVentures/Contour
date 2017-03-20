using Contour.Sending;

namespace Contour.Transport.RabbitMQ
{
    using Internal;

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
            var senderConfiguration = (SenderConfiguration)builder;
            var rabbitSenderOptions = (RabbitSenderOptions)senderConfiguration.Options;

            // Need to set the callback sender's connection string to use the same connection string both in sender and receiver in request-response scenario
            senderConfiguration.WithCallbackConnectionString(connectionString);
            rabbitSenderOptions.ConnectionString = connectionString;

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

        /// <summary>
        /// Configures sender to use <paramref name="policy"/> to select publisher each time a message is sent
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static ISenderConfigurator WithProducerSelector(this ISenderConfigurator builder,
            IProducerSelector selector)
        {
            var senderConfiguration = (SenderConfiguration)builder;
            var rabbitSenderOptions = (RabbitSenderOptions)senderConfiguration.Options;

            rabbitSenderOptions.ProducerSelector = selector;

            return builder;
        }
    }
}