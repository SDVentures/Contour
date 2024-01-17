namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitConnection : IChannelProvider<RabbitChannel>, IConnection
    {
        bool IsBlocked { get; }
    }
}
