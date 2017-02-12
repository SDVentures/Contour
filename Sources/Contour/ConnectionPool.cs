using System.Threading;

namespace Contour
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Common.Logging;

    internal abstract class ConnectionPool<TConnection> : IConnectionPool<TConnection> where TConnection : class, IConnection
    {
        private bool disposed;
        private readonly object syncRoot = new object();
        private readonly ILog logger = LogManager.GetLogger<ConnectionPool<TConnection>>();
        private int lastKey;
        private readonly ConcurrentDictionary<int, TConnection> connections =
            new ConcurrentDictionary<int, TConnection>();
        protected IConnectionProvider<TConnection> Provider;

        protected ConnectionPool(int maxSize)
        {
            MaxSize = maxSize <= 0 ? int.MaxValue : maxSize;
        }

        public int MaxSize { get; }

        public int Count => connections.Count;

        public event EventHandler ConnectionOpened;

        public event EventHandler ConnectionClosed;

        public event EventHandler ConnectionDisposed;

        public TConnection Get(CancellationToken token)
        {
            lock (syncRoot)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(nameof(ConnectionPool<TConnection>));
                }

                var random = new Random();
                var key = Count < MaxSize ? lastKey++ : random.Next(MaxSize);

                return connections.GetOrAdd(key, i =>
                {
                    var con = Provider.Create();
                    logger.Trace($"New connection [{con.Id}] created");

                    con.Opened += OnConnectionOpened;
                    con.Closed += OnConnectionClosed;
                    con.Disposed += OnConnectionDisposed;
                    con.Open(token);
                    return con;
                });
            }
        }

        public void Drop()
        {
            lock (syncRoot)
            {
                TConnection con;
                while (connections.Count > 0 && connections.TryRemove(connections.Keys.First(), out con))
                {
                    try
                    {
                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Failed to dispose a pooled connection due to: {ex.Message}", ex);
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                disposed = true;
                Drop();
            }
        }

        protected virtual void OnConnectionOpened(object sender, EventArgs args)
        {
            ConnectionOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConnectionClosed(object sender, EventArgs args)
        {
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConnectionDisposed(object sender, EventArgs args)
        {
            lock (syncRoot)
            {
                var connection = sender as TConnection;
                if (connection != null)
                {
                    var key = connections.FirstOrDefault(c => c.Value == connection).Key;

                    TConnection removedConnection;
                    if (connections.TryRemove(key, out removedConnection))
                    {
                        logger.Trace($"Connection [{key},{removedConnection.Id}] removed from connection pool");
                    }
                }
            }

            ConnectionDisposed?.Invoke(this, EventArgs.Empty);
        }
    }
}