using System;
using System.Threading;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitConnection : IChannelProvider<RabbitChannel>, IDisposable
    {
        RabbitBus Bus { get; }

        event EventHandler<ChannelFailureEventArgs> ChannelFailed;

        event EventHandler Opened;

        event EventHandler Closed;

        void Open(CancellationToken token);

        void Close();
    }
}