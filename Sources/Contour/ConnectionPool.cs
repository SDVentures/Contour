namespace Contour
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using Common.Logging;

    internal abstract class ConnectionPool<TConnection> : IConnectionPool<TConnection> where TConnection : class, IConnection
    {
        private readonly object syncRoot = new object();
        private readonly ILog logger = LogManager.GetLogger<ConnectionPool<TConnection>>();
        private readonly ConcurrentDictionary<int, TConnection> connections =
            new ConcurrentDictionary<int, TConnection>();

        private bool disposed;
        private int lastKey;
        private CancellationTokenSource cancellation;
        
        protected ConnectionPool(int maxSize)
        {
            this.cancellation = new CancellationTokenSource();
            this.MaxSize = maxSize <= 0 ? int.MaxValue : maxSize;
        }

        public event EventHandler ConnectionOpened;

        public event EventHandler ConnectionClosed;

        public event EventHandler ConnectionDisposed;

        public int MaxSize { get; }

        public int Count => this.connections.Count;

        protected IConnectionProvider<TConnection> Provider { get; set; }

        /// <summary>
        /// Gets a new connection from the pool or uses an existing one if <see cref="MaxSize"/> has been reached
        /// </summary>
        /// <param name="token">Operation cancellation token</param>
        /// <returns>A pooled connection</returns>
        public TConnection Get(CancellationToken token)
        {
            lock (this.syncRoot)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(nameof(ConnectionPool<TConnection>));
                }
                
                var random = new Random();
                var key = this.Count < this.MaxSize ? this.lastKey++ : random.Next(this.MaxSize);

                var source = CancellationTokenSource.CreateLinkedTokenSource(token, this.cancellation.Token);
                if (source.Token.IsCancellationRequested)
                {
                    return null;
                }

                return this.connections.GetOrAdd(
                    key,
                    i =>
                        {
                            var con = this.Provider.Create();
                            this.logger.Trace($"New connection [{con.Id}] created");

                            con.Opened += this.OnConnectionOpened;
                            con.Closed += this.OnConnectionClosed;
                            con.Disposed += this.OnConnectionDisposed;
                            con.Open(source.Token);
                            return con;
                        });
            }
        }

        /// <summary>
        /// Cancels any pending connection requests and drops all connections
        /// </summary>
        public void Drop()
        {
            // Cancel any pending connection requests
            this.cancellation.Cancel();

            lock (this.syncRoot)
            {
                TConnection con;
                while (this.connections.Count > 0 && this.connections.TryRemove(this.connections.Keys.First(), out con))
                {
                    try
                    {
                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Warn($"Failed to dispose a pooled connection due to: {ex.Message}", ex);
                    }
                }

                // Re-enable the clients to get new connections from the pool
                this.cancellation = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Drops all connections and disposes of the pool
        /// </summary>
        public void Dispose()
        {
            lock (this.syncRoot)
            {
                this.disposed = true;
                this.Drop();
            }
        }

        /// <summary>
        /// Invokes the <see cref="ConnectionOpened"/> event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Connection opened event arguments</param>
        protected virtual void OnConnectionOpened(object sender, EventArgs args)
        {
            this.ConnectionOpened?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="ConnectionClosed"/> event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Connection closed event arguments</param>
        protected virtual void OnConnectionClosed(object sender, EventArgs args)
        {
            this.ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="ConnectionDisposed"/> event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Connection disposed event arguments</param>
        protected virtual void OnConnectionDisposed(object sender, EventArgs args)
        {
            lock (this.syncRoot)
            {
                var connection = sender as TConnection;
                if (connection != null)
                {
                    var key = this.connections.FirstOrDefault(c => c.Value == connection).Key;

                    TConnection removedConnection;
                    if (this.connections.TryRemove(key, out removedConnection))
                    {
                        this.logger.Trace($"Connection [{key},{removedConnection.Id}] removed from connection pool");
                    }
                }
            }

            this.ConnectionDisposed?.Invoke(this, EventArgs.Empty);
        }
    }
}