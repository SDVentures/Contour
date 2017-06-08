namespace Contour.Transport.RabbitMq.Internal
{
    internal interface IRabbitConnection : IChannelProvider<IRabbitChannel>, IConnection
    {
    }
}