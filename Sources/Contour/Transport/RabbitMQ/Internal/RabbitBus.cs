using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Configuration;
using Contour.Helpers;
using Contour.Receiving;
using Contour.Transport.RabbitMQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// The rabbit bus.
    /// </summary>
    internal class RabbitBus : AbstractBus, IBusAdvanced, IChannelProvider
    {
        private readonly ILog logger = LogManager.GetLogger<RabbitBus>();

        private readonly ManualResetEvent isRestarting = new ManualResetEvent(false);
        private readonly ManualResetEvent ready = new ManualResetEvent(false);

        private CancellationTokenSource cancellationTokenSource;
        private Task restartTask;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitBus" />.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RabbitBus(BusConfiguration configuration)
            : base(configuration)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var completion = new TaskCompletionSource<object>();
            completion.SetResult(new object());
            this.restartTask = completion.Task;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public IConnection Connection { get; private set; }

        /// <summary>
        /// Gets the producer registry.
        /// </summary>
        public ProducerRegistry ProducerRegistry { get; private set; }

        /// <summary>
        /// Gets the when ready.
        /// </summary>
        public override WaitHandle WhenReady
        {
            get
            {
                return this.ready;
            }
        }

        /// <summary>
        /// The open channel.
        /// </summary>
        /// <returns>The <see cref="IChannel" />.</returns>
        IChannel IBusAdvanced.OpenChannel()
        {
            return this.OpenChannel();
        }

        /// <summary>
        /// The panic.
        /// </summary>
        void IBusAdvanced.Panic()
        {
            this.Restart();
        }

        /// <summary>
        /// The open channel.
        /// </summary>
        /// <returns>The <see cref="IChannel" />.</returns>
        IChannel IChannelProvider.OpenChannel()
        {
            return this.OpenChannel();
        }

        /// <summary>
        /// The open channel.
        /// </summary>
        /// <returns>The <see cref="RabbitChannel" />.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual RabbitChannel OpenChannel()
        {
            if (this.Connection == null || !this.Connection.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ Connection is not open.");
            }

            try
            {
                var channel = new RabbitChannel(this, this.Connection.CreateModel());
                channel.Failed += (ch, args) => this.HandleBusFailure(args.GetException());
                return channel;
            }
            catch (Exception ex)
            {
                this.HandleBusFailure(ex);
                throw;
            }
        }

        /// <summary>
        /// The shutdown.
        /// </summary>
        public override void Shutdown()
        {
            this.logger.InfoFormat(
                "Shutting down [{0}] with endpoint [{1}].",
                this.GetType().Name,
                this.Endpoint);

            this.IsShuttingDown = true;
            this.Stop();
            
            if (this.ProducerRegistry != null)
            {
                this.logger.Trace(m => m("{0}: disposing producer registry.", this.Endpoint));
                this.ProducerRegistry.Dispose();
                this.ProducerRegistry = null;
            }

            this.logger.Trace(m => m("{0}: resetting state.", this.Endpoint));
            this.IsConfigured = false;

            // если не ожидать завершения задачи до сброса флага IsShuttingDown,
            // тогда в случае ошибок (например, когда обработчик пытается отправить сообщение в шину, а она в состоятии закрытия)
            // задача может не успеть закрыться и она входит в бесконечное ожидание в методе Restart -> ResetRestartTask.
            this.restartTask.Wait();
            this.IsShuttingDown = false;
        }

        /// <summary>
        /// The start.
        /// </summary>
        /// <param name="waitForReadiness">The wait for readiness.</param>
        /// <exception cref="AggregateException"></exception>
        public override void Start(bool waitForReadiness = true)
        {
            if (this.IsStarted || this.IsShuttingDown)
            {
                return;
            }

            this.Restart(waitForReadiness);
        }

        private void ResetRestartTask()
        {
            if (!this.restartTask.IsCompleted)
            {
                this.cancellationTokenSource.Cancel();
                try
                {
                    this.restartTask.Wait();
                }
                catch (AggregateException ex)
                {
                    ex.Handle(
                        e =>
                            {
                                this.logger.ErrorFormat("{0}: Caught unexpected exception.", e, this.Endpoint);
                                return true;
                            });
                }
                catch (Exception ex)
                {
                    this.logger.ErrorFormat("{0}: Caught unexpected exception.", ex, this.Endpoint);
                }
                finally
                {
                    this.cancellationTokenSource = new CancellationTokenSource();
                }
            }
        }

        public override void Stop()
        {
            this.ResetRestartTask();

            var token = this.cancellationTokenSource.Token;
            this.restartTask = Task.Factory.StartNew(() => this.StopTask(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            this.restartTask.Wait(5000);
        }

        private void BuildReceivers()
        {
            this.Configuration.ReceiverConfigurations.ForEach(
                config =>
                    {
                        var receiver = new RabbitReceiver(this, config);
                        this.ComponentTracker.Register(receiver);
                    });
        }

        private void BuildSenders()
        {
            this.ProducerRegistry = new ProducerRegistry(this);

            this.Configuration.SenderConfigurations.ForEach(
                c =>
                    {
                        var sender = new RabbitSender(this.Configuration.Endpoint, c, this.ProducerRegistry, this.Configuration.Filters.ToList());
                        this.ComponentTracker.Register(sender);
                    });
        }

        private void Configure()
        {
            if (this.IsConfigured)
            {
                return;
            }

            this.logger.InfoFormat(
                "Configuring [{0}] with endpoint [{1}].".FormatEx(
                    this.GetType()
                        .Name,
                    this.Endpoint));
            
            this.BuildReceivers();
            this.BuildSenders();

            this.IsConfigured = true;
        }

        private void Connect(CancellationToken token)
        {
            this.logger.InfoFormat("Connecting to RabbitMQ using [{0}].", this.Configuration.ConnectionString);

            var clientProperties = new Dictionary<string, object>
            {
                                           { "Endpoint", this.Endpoint.Address },
                                           { "Machine", Environment.MachineName },
                                           {
                                               "Location", Path.GetDirectoryName(
                                                   Assembly.GetExecutingAssembly()
                                               .
                                               CodeBase)
                                           }
                                       };

            var connectionFactory = new ConnectionFactory
                                        {
                                            Uri = this.Configuration.ConnectionString,
                                            ClientProperties = clientProperties,
                                            RequestedConnectionTimeout = 3000 // 3s
                                        };

            var retryCount = 0;
            while (!token.IsCancellationRequested)
            {
                IConnection newConnection = null;
                try
                {
                    newConnection = connectionFactory.CreateConnection();
                    newConnection.ConnectionShutdown += this.DisconnectEventHandler;
                    this.Connection = newConnection;
                    this.OnConnected();
                    return;
                }
                catch (Exception ex)
                {
                    var secondsToRetry = Math.Min(10, retryCount);

                    this.logger.WarnFormat("Unable to connect to RabbitMQ. Retrying in {0} seconds...", ex, secondsToRetry);

                    if (newConnection != null)
                    {
                        newConnection.ConnectionShutdown -= this.DisconnectEventHandler;
                        newConnection.Abort(500);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(secondsToRetry));
                    retryCount++;
                }
            }
        }

        private void DisconnectEventHandler(IConnection connection, ShutdownEventArgs eventArgs)
        {
            Task.Factory.StartNew(
                () =>
                    {
                        connection.ConnectionShutdown -= this.DisconnectEventHandler;
                        this.OnDisconnected();
                        this.HandleBusFailure(new BusConnectionException("Connection was shut down on [{0}].".FormatEx(this.Endpoint)));
                    });
        }

        private void DropConnection()
        {
            if (this.Connection != null)
            {
                if (this.Connection.CloseReason == null)
                {
                    this.logger.Trace(m => m("{0}: closing connection.", this.Endpoint));
                    try
                    {
                        this.Connection.Close(500);
                        if (this.Connection.CloseReason != null)
                        {
                            this.Connection.Abort(500);
                        }
                    }
                    catch (AlreadyClosedException ex)
                    {
                        this.logger.WarnFormat("{0}: connection is already closed. Possible race condition.", ex, this.Endpoint);
                    }

                    this.logger.Trace(m => m("{0}: disposing connection.", this.Endpoint));
                }

                this.Connection = null;
            }
        }

        private void HandleBusFailure(Exception exception)
        {
            if (this.IsStarted && !this.IsShuttingDown)
            {
                // restarting only if not already stopping/stopped
                this.logger.ErrorFormat("Channel failure was detected. Trying to restart the bus instance [{0}].", exception, this.Endpoint);
                this.Restart();
            }
        }

        private void StopTask(CancellationToken cancellationToken)
        {
            if (!this.IsConfigured)
            {
                return;
            }

            this.logger.Trace(m => m("{0}: marking as not ready.", this.Endpoint));
            this.ready.Reset();

            this.OnStopping();

            this.logger.Trace(m => m("{0}: stopping bus components.", this.Endpoint));
            this.ComponentTracker.StopAll();

            if (this.ProducerRegistry != null)
            {
                this.logger.Trace(m => m("{0}: resetting producer registry.", this.Endpoint));
                this.ProducerRegistry.Reset();
            }

            this.DropConnection();

            this.OnStopped();
        }

        private void StartTask(CancellationToken cancellationToken)
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            this.logger.Trace(m => m("{0}: configuring.", this.Endpoint));
            this.Configure();

            this.OnStarting();

            this.Connect(cancellationToken);

            this.logger.Trace(m => m("{0}: starting components.", this.Endpoint));
            this.ComponentTracker.StartAll();

            this.logger.Trace(m => m("{0}: marking as ready.", this.Endpoint));
            this.IsStarted = true;
            this.ready.Set();

            this.OnStarted();
        }

        protected override void Restart(bool waitForReadiness = true)
        {
            lock (this.logger)
            {
                if (this.isRestarting.WaitOne(0) || this.IsShuttingDown)
                {
                    return;
                }
                this.ready.Reset();
                this.isRestarting.Set();
            }

            this.logger.Trace(m => m("{0}: Restarting...", this.Endpoint));

            this.ResetRestartTask();

            var token = this.cancellationTokenSource.Token;
            this.restartTask = Task.Factory.StartNew(() => this.StopTask(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(_ => this.StartTask(token), token, TaskContinuationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(
                    t =>
                        {
                            this.isRestarting.Reset();
                            if (t.IsFaulted)
                            {
                                throw t.Exception.InnerException;
                            }
                        });

            if (waitForReadiness)
            {
                this.restartTask.Wait(5000);
            }
        }
    }
}