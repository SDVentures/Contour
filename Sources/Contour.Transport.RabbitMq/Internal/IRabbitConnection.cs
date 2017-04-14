namespace Contour.Transport.RabbitMq.Internal
{
    internal interface IRabbitConnection : IChannelProvider<RabbitChannel>, IConnection
    {
    }
}