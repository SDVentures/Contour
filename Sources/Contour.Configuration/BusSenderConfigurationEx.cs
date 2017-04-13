using Contour.Sending;
using Contour.Transport.RabbitMq;

namespace Contour.Configuration
{
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
        /// The <see cref="Contour.Sending.ISenderConfigurator"/>.
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
        /// The <see cref="Contour.Sending.ISenderConfigurator"/>.
        /// </returns>
        public static ISenderConfigurator ReuseConnection(this ISenderConfigurator builder, bool reuse = true)
        {
            ((RabbitSenderOptions)((SenderConfiguration)builder).Options).ReuseConnection = reuse;
            return builder;
        }
    }
}