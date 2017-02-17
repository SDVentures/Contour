using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly ILog logger = LogManager.GetLogger<RabbitConnection>();
        private readonly IEndpoint endpoint;
        private readonly IBusContext busContext;
        private INativeConnection connection;
        
        public RabbitConnection(IEndpoint endpoint, string connectionString, IBusContext busContext)
        {
            this.Id = Guid.NewGuid();
            this.endpoint = endpoint;
            this.ConnectionString = connectionString;
            this.busContext = busContext;
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public event EventHandler Disposed;

        public Guid Id { get; }

        public string ConnectionString { get; }

        public void Open(CancellationToken token)
        {
            this.logger.Info($"Connecting to RabbitMQ using [{this.ConnectionString}].");

            var clientProperties = new Dictionary<string, object>
                                       {
                                           {
                                               "Endpoint",
                                               this.endpoint.Address
                                           },
                                           { "Machine", Environment.MachineName },
                                           {
                                               "Location",
                                               Path.GetDirectoryName(
                                                   Assembly.GetExecutingAssembly().CodeBase)
                                           }
                                       };

            var connectionFactory = new ConnectionFactory
                                        {
                                            Uri = this.ConnectionString,
                                            ClientProperties = clientProperties,
                                            RequestedConnectionTimeout = ConnectionTimeout
                                        };

            var retryCount = 0;
            while (!token.IsCancellationRequested)
            {
                INativeConnection con = null;
                try
                {
                    con = connectionFactory.CreateConnection();
                    con.ConnectionShutdown += this.OnConnectionShutdown;
                    this.connection = con;
                    this.OnOpened();
                    this.logger.Info($"Connection [{this.Id}] opened at [{this.connection.Endpoint}]");

                    return;
                }
                catch (Exception ex)
                {
                    var secondsToRetry = Math.Min(10, retryCount);

                    this.logger.WarnFormat(
                        "Unable to connect to RabbitMQ. Retrying in {0} seconds...",
                        ex,
                        secondsToRetry);

                    if (con != null)
                    {
                        con.ConnectionShutdown -= this.OnConnectionShutdown;
                        con.Abort(OperationTimeout);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(secondsToRetry));
                    retryCount++;
                }
            }
        }

        public RabbitChannel OpenChannel()
        {
            if (this.connection == null || !this.connection.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ connection is not open.");
            }
            try
            {
                var model = this.connection.CreateModel();
                var channel = new RabbitChannel(model, this.busContext);
                channel.Failed += (ch, args) => this.OnChannelFailed(ch, args.GetException());
                return channel;
            }
            catch (Exception ex)
            {
                this.logger.Error($"Failed to open a new channel in connection [{this}] due to: {ex.Message}", ex);
                throw;
            }
        }

        public void Close()
        {
            if (this.connection != null)
            {
                if (this.connection.CloseReason == null)
                {
                    this.logger.Trace($"[{this.endpoint}]: closing connection.");
                    try
                    {
                        this.connection.Close(OperationTimeout);
                        if (this.connection.CloseReason != null)
                        {
                            this.connection.Abort(OperationTimeout);
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

        public void Abort()
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

        public void Dispose()
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
            finally
            {
                this.OnDisposed();
            }
        }

        public override string ToString()
        {
            return $"[{this.Id} : {this.ConnectionString}]";
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

        protected virtual void OnChannelFailed(IChannel channel, Exception channelException)
        {
            this.logger.Warn($"A channel in connection [{this}] has failed: {channelException.Message}. Connection is going to be closed");

            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                this.logger.Warn($"Failed to close a connection [{this}] due to {ex.Message}", ex);
            }
        }
        
        private void OnConnectionShutdown(INativeConnection conn, ShutdownEventArgs eventArgs)
        {
            Task.Factory.StartNew(
                () =>
                {
                    conn.ConnectionShutdown -= this.OnConnectionShutdown;
                    this.OnClosed();
                });
        }
    }
}