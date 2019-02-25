// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BusConsumerConfigurationEx.cs" company="">
//   
// </copyright>
// <summary>
//   The bus consumer configuration ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Contour.Helpers;
using Contour.Receiving;

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// Rabbit receiver configuration extensions
    /// </summary>
    public static class BusReceiverConfigurationEx
    {
        /// <summary>
        /// The with qo s.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="qosParams">
        /// The qos params.
        /// </param>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator WithQoS(this IReceiverConfigurator builder, QoSParams qosParams)
        {
            ((RabbitReceiverOptions)((ReceiverConfiguration)builder).Options).QoS = qosParams;

            return builder;
        }

        /// <summary>
        /// Returns current QoS settings
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Maybe<QoSParams> GetQoS(this IReceiverConfigurator builder)
        {
            var config = builder as ReceiverConfiguration;
            var options = config?.Options as RabbitReceiverOptions;
            return options != null ? options.GetQoS() : Maybe<QoSParams>.Empty;
        }

        /// <summary>
        /// The with qo s.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="qosParams">
        /// The qos params.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator<T> WithQoS<T>(this IReceiverConfigurator<T> builder, QoSParams qosParams)
            where T : class
        {
            ReceiverConfiguration configuration = ((TypedReceiverConfigurationDecorator<T>)builder).Configuration;

            WithQoS(configuration, qosParams);

            return builder;
        }

        /// <summary>
        /// The with qo s.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="prefetchCount">
        /// The prefetch count.
        /// </param>
        /// <param name="prefetchSize">
        /// The prefetch size.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator<T> WithQoS<T>(this IReceiverConfigurator<T> builder, ushort prefetchCount,
            uint prefetchSize) where T : class
        {
            return builder.WithQoS(new QoSParams(prefetchCount, prefetchSize));
        }

        /// <summary>
        /// Configure receiver to use a connection string.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator WithConnectionString(this IReceiverConfigurator builder,
            string connectionString)
        {
            var receiverConfiguration = (ReceiverConfiguration)builder;
            var rabbitReceiverOptions = (RabbitReceiverOptions)receiverConfiguration.Options;

            // The connection string should not be overridden if the connection string provider is present and it does not return nulls
            var provider = rabbitReceiverOptions.GetConnectionStringProvider();
            if (!string.IsNullOrEmpty(provider?.GetConnectionString(receiverConfiguration.Label)))
            {
                return builder;
            }

            rabbitReceiverOptions.ConnectionString = connectionString;
            return builder;
        }

        /// <summary>
        /// Configure receiver to use a connection string.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator<T> WithConnectionString<T>(this IReceiverConfigurator<T> builder,
            string connectionString) where T : class
        {
            var configuration = ((TypedReceiverConfigurationDecorator<T>)builder).Configuration;
            WithConnectionString(configuration, connectionString);

            return builder;
        }

        /// <summary>
        /// Configures receiver to reuse a connection if possible
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="reuse">
        /// The reuse.
        /// </param>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator ReuseConnection(this IReceiverConfigurator builder, bool reuse = true)
        {
            var configuration = builder as ReceiverConfiguration;
            var options = configuration?.Options as RabbitReceiverOptions;
            if (options != null)
            {
                options.ReuseConnection = reuse;
            }

            return builder;
        }

        /// <summary>
        /// Configures receiver to reuse a connection if possible.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="reuse">
        /// Connection reuse flag.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IReceiverConfigurator"/>.
        /// </returns>
        public static IReceiverConfigurator<T> ReuseConnection<T>(this IReceiverConfigurator<T> builder,
            bool reuse = true) where T : class
        {
            var configuration = ((TypedReceiverConfigurationDecorator<T>)builder).Configuration;
            ReuseConnection(configuration, reuse);

            return builder;
        }
    }
}
