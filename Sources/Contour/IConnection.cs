using System;
using System.Threading;
using Contour.Transport.RabbitMQ.Internal;

namespace Contour
{
    public interface IConnection: IDisposable
    {
        Guid Id { get; }

        event EventHandler Opened;

        event EventHandler Closed;

        event EventHandler Disposed;

        event EventHandler<ChannelFailedEventArgs> ChannelFailed;

        void Open(CancellationToken token);

        void Close();

        void Abort();
    }
}