using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitConnection : IChannelProvider<RabbitChannel>, IConnection
    {
        RabbitBus Bus { get; }

        event EventHandler<ChannelFailureEventArgs> ChannelFailed;
    }
}