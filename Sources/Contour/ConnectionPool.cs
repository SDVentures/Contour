namespace Contour
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Logging;

    internal abstract class ConnectionPool<TConnection> : IConnectionPool<TConnection> where TConnection : class, IConnection
    {
        private readonly object syncRoot = new object();
        private readonly ILog logger;
        private readonly ConcurrentDictionary<string, IList<Tuple<TConnection, bool>>> groups =
            new ConcurrentDictionary<string, IList<Tuple<TConnection, bool>>>();

        private bool disposed;
        private CancellationTokenSource cancellation;
        
        protected ConnectionPool()
        {
            this.logger = LogManager.GetLogger(this.GetType().FullName);
            this.cancellation = new CancellationTokenSource();
        }

        public event EventHandler ConnectionOpened;

        public event EventHandler ConnectionClosed;

        public event EventHandler ConnectionDisposed;
        
        public int Count => this.groups.SelectMany(pair => pair.Value).Count();

        protected IConnectionProvider<TConnection> Provider { get; set; }

        /// <summary>
        /// Gets a new connection from the pool or uses an existing one
        /// </summary>
        /// <param name="connectionString">A connection string to create a new or get an existing connection</param>
        /// <param name="reusable">Specifies if a connection can be reused</param>
        /// <param name="token">Operation cancellation token</param>
        /// <returns>A pooled connection</returns>
        public TConnection Get(string connectionString, bool reusable, CancellationToken token)
        {
            lock (this.syncRoot)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(typeof(ConnectionPool<TConnection>).Name);
                }
                
                var source = CancellationTokenSource.CreateLinkedTokenSource(token, this.cancellation.Token);
                if (source.Token.IsCancellationRequested)
                {
                    this.logger.Trace("Connection pool Get operation has already been canceled");
                    return null;
                }

                var group = this.groups.GetOrAdd(connectionString, s => new List<Tuple<TConnection, bool>>());

                Tuple<TConnection, bool> pair;

                if (reusable && group.Any(t => t.Item2))
                {
                    pair = group.First(t => t.Item2);
                    this.logger.Trace($"A reusable connection [{pair.Item1}] has been fetched from the pool");
                }
                else
                {
                    var connection = this.Provider.Create(connectionString);
                    this.logger.Trace($"A new connection [{connection}] has been created");

                    pair = new Tuple<TConnection, bool>(connection, reusable);
                    group.Add(pair);

                    connection.Opened += this.OnConnectionOpened;
                    connection.Closed += this.OnConnectionClosed;
                    connection.Disposed += this.OnConnectionDisposed;
                    connection.Open(source.Token);
                    this.logger.Trace($"Connection [{connection}] has been opened");
                }
                
                return pair.Item1;
            }
        }

        /// <summary>
        /// Cancels any pending connection requests and drops all connections
        /// </summary>
        public void Drop()
        {
            this.logger.Trace("Dropping connection pool...");

            // Cancel any pending connection requests
            this.cancellation.Cancel();
            this.logger.Trace("All pending connection requests have been canceled");

            lock (this.syncRoot)
            {
                IList<Tuple<TConnection, bool>> group;
                while (this.groups.Count > 0 && this.groups.TryRemove(this.groups.Keys.First(), out group))
                {
                    this.logger.Trace("Cleaning up connection group...");

                    while (group.Any())
                    {
                        var pair = group.First();
                        try
                        {
                            var connection = pair.Item1;

                            this.logger.Trace($"Closing connection [{connection}]...");
                            connection.Close();
                            this.logger.Trace($"Connection [{connection}] closed");

                            this.logger.Trace($"Disposing connection [{connection}]...");
                            connection.Dispose();
                            this.logger.Trace($"Connection [{connection}] disposed");
                        }
                        catch (Exception ex)
                        {
                            this.logger.Warn($"Failed to dispose a pooled connection due to: {ex.Message}", ex);
                        }
                        finally
                        {
                            group.Remove(pair);
                        }
                    }

                    this.logger.Trace("Connection group cleanup successfully completed");
                }

                // Re-enable the clients to get new connections from the pool
                this.cancellation = new CancellationTokenSource();
            }

            this.logger.Trace("Connection pool successfully dropped");
        }

        /// <summary>
        /// Drops all connections and disposes of the pool
        /// </summary>
        public void Dispose()
        {
            this.logger.Trace("Disposing connection pool...");

            lock (this.syncRoot)
            {
                this.disposed = true;
                this.Drop();
            }

            this.logger.Trace("Connection pool successfully disposed");
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
                    IList<Tuple<TConnection, bool>> group;
                    if (this.groups.TryGetValue(connection.ConnectionString, out group))
                    {
                        var pair = group.First(p => p.Item1 == connection);
                        group.Remove(pair);
                        this.logger.Trace(
                            $"Connection [{connection.ConnectionString},{connection.Id}] removed from connection pool");
                    }
                }
            }

            this.ConnectionDisposed?.Invoke(this, EventArgs.Empty);
        }
    }
}