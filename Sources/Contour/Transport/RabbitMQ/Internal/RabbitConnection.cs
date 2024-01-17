using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using INativeConnection = RabbitMQ.Client.IConnection;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnection : IRabbitConnection
    {
        private const int ConnectionTimeout = 3000;
        private const int OperationTimeout = 500;
        private readonly object syncRoot = new object();
        private readonly ILog logger = LogManager.GetLogger<RabbitConnection>();
        private readonly IEndpoint endpoint;
        private readonly IBusContext busContext;
        private readonly ConnectionFactory connectionFactory;
        private INativeConnection connection;

        public RabbitConnection(IEndpoint endpoint, string connectionString, IBusContext busContext)
        {
            this.Id = Guid.NewGuid();
            this.endpoint = endpoint;
            this.ConnectionString = connectionString;
            this.busContext = busContext;

            this.ConnectionKey = GetConnectionKey(connectionString);

            var clientProperties = new Dictionary<string, object>
            {
                { "Endpoint", this.endpoint.Address },
                { "Machine", Environment.MachineName },
                { "Location", Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) },
                { "ConnectionId", this.Id.ToString() }
            };

            this.connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(this.ConnectionString),
                AutomaticRecoveryEnabled = false,
                ClientProperties = clientProperties,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(ConnectionTimeout)
            };
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public event EventHandler Disposed;

        public Guid Id { get; }

        public string ConnectionString { get; }

        public string ConnectionKey { get; }

        public void Open(CancellationToken token)
        {
            lock (this.syncRoot)
            {
                if (this.connection?.IsOpen ?? false)
                {
                    this.logger.Trace($"Connection [{this.Id}] is already open");
                    return;
                }

                this.logger.Info($"Connecting to RabbitMQ using [{this.ConnectionString}]");

                var retryCount = 0;
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    INativeConnection con = null;
                    try
                    {
                        con = this.connectionFactory.CreateConnection();
                        con.ConnectionShutdown += this.OnConnectionShutdown;
                        
                        con.ConnectionBlocked += OnConnectionBlocked;
                        con.ConnectionUnblocked += OnConnectionUnblocked;
                        this.connection = con;
                        this.OnOpened();
                        this.logger.Info($"Connection [{this.Id}] opened at [{this.connection.Endpoint}]");

                        return;
                    }
                    catch (Exception ex)
                    {
                        if (con != null)
                        {
                            con.ConnectionShutdown -= this.OnConnectionShutdown;
                            con.Abort(TimeSpan.FromMilliseconds(OperationTimeout));
                        }

                        var secondsToRetry = Math.Min(10, retryCount);

                        this.logger.Warn(m => m("Unable to connect to RabbitMQ on connection string: [{1}]. Retrying in {0} seconds...", secondsToRetry, this.ConnectionString), ex);
                        
                        Thread.Sleep(TimeSpan.FromSeconds(secondsToRetry));
                        retryCount++;
                    }
                }
            }
        }

        public bool IsBlocked { get; private set; }

        private void OnConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            // todo: reduce logging level later
            this.logger.Warn(m => m($"OnConnectionBlocked {sender}. Reason: {e.Reason}"));
            IsBlocked = true;
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            // todo: reduce logging level later
            this.logger.Warn(m => m($"OnConnectionUnblocked {sender}"));
            IsBlocked = false;
        }

        public RabbitChannel OpenChannel(CancellationToken token)
        {
            lock (this.syncRoot)
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    if (this.connection == null || !this.connection.IsOpen)
                    {
                        this.Open(token);
                    }

                    try
                    {
                        var model = this.connection.CreateModel();
                        var channel = new RabbitChannel(this.Id, model, this.busContext, this.ConnectionString, this.ConnectionKey);
                        return channel;
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Failed to open a new channel on connection string: [{this.ConnectionString}] due to: {ex.Message}; retrying...", ex);
                    }
                }
            }
        }

        public void Close()
        {
            lock (this.syncRoot)
            {
                if (this.connection != null)
                {
                    if (this.connection.CloseReason == null)
                    {
                        this.logger.Trace($"[{this.endpoint}]: closing connection.");
                        try
                        {
                            this.connection.Close(TimeSpan.FromMilliseconds(OperationTimeout));
                            if (this.connection.CloseReason != null)
                            {
                                this.connection.Abort(TimeSpan.FromMilliseconds(OperationTimeout));
                            }
                        }
                        catch (AlreadyClosedException ex)
                        {
                            this.logger.Warn(
                                $"[{this.endpoint}]: connection is already closed: {ex.Message}");
                        }
                    }
                }
            }
        }

        public void Abort()
        {
            lock (this.syncRoot)
            {
                try
                {
                    this.connection?.Abort();
                }
                catch (Exception ex)
                {
                    this.logger.Error(
                        $"[{this.endpoint}]: failed to abort the underlying connection due to: {ex.Message}",
                        ex);
                    throw;
                }
            }
        }

        public void Dispose()
        {
            lock (this.syncRoot)
            {
                try
                {
                    this.Close();

                    if (this.connection != null)
                    {
                        this.logger.Trace(
                            $"[{this.endpoint}]: disposing connection [{this.Id}] at [{this.connection.Endpoint}].");
                        this.connection?.Dispose();
                        this.connection = null;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Trace($"An error '{ex.Message}' during connection cleanup has been suppressed");
                }
                finally
                {
                    this.OnDisposed();
                }
            }
        }

        public override string ToString()
        {
            return $"{this.Id} : {this.ConnectionString} : {this.ConnectionKey}";
        }

        protected virtual void OnOpened()
        {
            this.Opened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosed()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisposed()
        {
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            Task.Factory.StartNew(
                () =>
                {
                    this.logger.Trace($"Connection [{this.Id}] has been closed due to {eventArgs.ReplyText} ({eventArgs.ReplyCode})");

                    lock (this.syncRoot)
                    {
                        ((INativeConnection)sender).ConnectionShutdown -= this.OnConnectionShutdown;
                    }

                    this.OnClosed();
                });
        }


        // TODO вынести знание о DNS и прочем за пределы контура, вычислять данные вне контура и передавать на старте
        private static string GetConnectionKey(string connectionString)
        {
            var uri = new Uri(connectionString, UriKind.Absolute);

            var ip = string.Join(",", Dns.GetHostAddresses(uri.Host).OrderBy(x => x.ToString()));

            return $"{ip}:{uri.Port}:{uri.Segments.Last()}";
        }
    }
}
