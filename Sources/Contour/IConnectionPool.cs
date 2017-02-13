using System;
using System.Threading;

namespace Contour
{
    internal interface IConnectionPool<out TConnection> : IDisposable where TConnection : class, IConnection
    {
        event EventHandler ConnectionOpened;

        event EventHandler ConnectionClosed;

        event EventHandler ConnectionDisposed;

        int MaxSize { get; }

        int Count { get; }

        TConnection Get(CancellationToken token);

        void Drop();
    }
}