using System;
using System.Threading;

namespace Contour
{
    internal interface IConnectionPool<out TConnection> : IDisposable where TConnection : class, IConnection
    {
        int MaxSize { get; }

        int Count { get; }

        event EventHandler ConnectionOpened;

        event EventHandler ConnectionClosed;

        event EventHandler ConnectionDisposed;

        TConnection Get(CancellationToken token);

        void Drop();
    }
}