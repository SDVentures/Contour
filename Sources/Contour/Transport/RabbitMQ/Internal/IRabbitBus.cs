using RabbitMQ.Client;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitBus: IBus, IBusAdvanced, IChannelProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        IConnection Connection { get; }
    }
}