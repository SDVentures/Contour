using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Contour.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Contour.Transport.RabbitMQ.Internal
{
    class RabbitConnection : IRabbitConnection
    {
        private const int ConnectionTimeout = 3000;
        private const int OperationTimeout = 500;
        private readonly ILog logger = LogManager.GetLogger<RabbitConnection>();
        private IConnection connection;

        public RabbitConnection(RabbitBus bus)
        {
            this.Bus = bus;
        }

        public RabbitBus Bus { get; }

        public event EventHandler<ChannelFailureEventArgs> ChannelFailed;

        public event EventHandler Opened;

        public event EventHandler Closed;

        public void Open(CancellationToken token)
        {
            this.logger.Info($"Connecting to RabbitMQ using [{this.Bus.Configuration.ConnectionString}].");

            var clientProperties = new Dictionary<string, object>
            {
                {"Endpoint", this.Bus.Configuration.Endpoint.Address},
                {"Machine", Environment.MachineName},
                {"Location", Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)}
            };

            var connectionFactory = new ConnectionFactory
            {
                Uri = this.Bus.Configuration.ConnectionString,
                ClientProperties = clientProperties,
                RequestedConnectionTimeout = ConnectionTimeout
            };

            var retryCount = 0;
            while (!token.IsCancellationRequested)
            {
                IConnection con = null;
                try
                {
                    con = connectionFactory.CreateConnection();
                    con.ConnectionShutdown += this.OnConnectionShutdown;
                    this.connection = con;
                    this.OnOpened();

                    return;
                }
                catch (Exception ex)
                {
                    var secondsToRetry = Math.Min(10, retryCount);

                    this.logger.WarnFormat("Unable to connect to RabbitMQ. Retrying in {0} seconds...", ex,
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
                var channel = new RabbitChannel(Bus, this.connection.CreateModel());
                channel.Failed += (ch, args) => this.OnChannelFailed(args.GetException());
                return channel;
            }
            catch (Exception ex)
            {
                this.OnChannelFailed(ex);
                throw;
            }
        }

        public void Close()
        {
            if (this.connection != null)
            {
                if (this.connection.CloseReason == null)
                {
                    this.logger.Trace($"[{this.Bus.Configuration.Endpoint}]: closing connection.");
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
                        this.logger.Warn($"[{this.Bus.Configuration.Endpoint}]: connection is already closed: {ex.Message}");
                    }

                    this.logger.Trace($"[{this.Bus.Configuration.Endpoint}]: disposing connection.");
                    this.connection.Dispose();
                }

                this.connection = null;
            }
        }

        private void OnConnectionShutdown(IConnection conn, ShutdownEventArgs eventArgs)
        {
            Task.Factory.StartNew(
                () =>
                {
                    conn.ConnectionShutdown -= this.OnConnectionShutdown;
                    this.OnClosed();
                    this.OnChannelFailed(
                        new BusConnectionException($"[{this.Bus.Configuration.Endpoint}]: connection was shut down."));
                });
        }

        protected virtual void OnOpened()
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnChannelFailed(Exception exception)
        {
            ChannelFailed?.Invoke(this, new ChannelFailureEventArgs(exception));
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}