using System;
using System.Threading;

namespace Contour
{
    internal interface IConnectionPool<out TConnection> : IDisposable where TConnection : class, IConnection
    {
        event EventHandler ConnectionOpened;

        event EventHandler ConnectionClosed;

        event EventHandler ConnectionDisposed;

        int Count { get; }

        TConnection Get(string connectionString, bool reusable, CancellationToken token);

        void Drop();
    }
}