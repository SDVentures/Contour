using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Configuration;
using Contour.Helpers;

using RabbitMQ.Client;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// The rabbit bus.
    /// </summary>
    internal class RabbitBus : AbstractBus, IBusAdvanced
    {
        private readonly ILog logger = LogManager.GetLogger<RabbitBus>();

        private readonly ManualResetEvent isRestarting = new ManualResetEvent(false);

        private readonly ManualResetEvent ready = new ManualResetEvent(false);

        private CancellationTokenSource cancellationTokenSource;

        private Task restartTask;

        private readonly IRabbitConnectionProvider connectionProvider;
        private readonly IRabbitConnection connection;

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

            this.connectionProvider = new RabbitConnectionProvider(this);
            this.connection = connectionProvider.Create();
            this.connection.Closed += this.Closed;
            this.connection.ChannelFailed += this.ChannelFailed;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public IConnection Connection { get; private set; }

        /// <summary>
        /// Gets the listener registry.
        /// </summary>
        public ListenerRegistry ListenerRegistry { get; private set; }

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
        /// The panic.
        /// </summary>
        public void Panic()
        {
            this.Restart();
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

            if (this.ListenerRegistry != null)
            {
                this.logger.Trace(m => m("{0}: disposing listener registry.", this.Endpoint));
                this.ListenerRegistry.Dispose();
                this.ListenerRegistry = null;
            }

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
            this.ListenerRegistry = new ListenerRegistry(connection);

            this.Configuration.ReceiverConfigurations.ForEach(
                c =>
                    {
                        var receiver = new RabbitReceiver(c, this.ListenerRegistry);
                        this.ComponentTracker.Register(receiver);
                    });
        }

        private void BuildSenders()
        {
            this.ProducerRegistry = new ProducerRegistry(connection);

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

        private void Closed(object sender, EventArgs args)
        {
            Task.Factory.StartNew(() =>
            {
                this.connection.Closed -= this.Closed;
                this.OnDisconnected();
                this.ChannelFailed(this,
                    new ChannelFailureEventArgs(
                        new BusConnectionException("Connection was shut down on [{0}].".FormatEx(this.Endpoint))));
            });
        }

        private void ChannelFailed(object sender, ChannelFailureEventArgs args)
        {
            if (this.IsStarted && !this.IsShuttingDown)
            {
                // restarting only if not already stopping/stopped
                this.logger.ErrorFormat("Channel failure was detected. Trying to restart the bus instance [{0}].",
                    args.Exception, this.Endpoint);
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

            if (this.ListenerRegistry != null)
            {
                this.logger.Trace(m => m("{0}: resetting listener registry.", this.Endpoint));
                this.ListenerRegistry.Reset();
            }

            if (this.ProducerRegistry != null)
            {
                this.logger.Trace(m => m("{0}: resetting producer registry.", this.Endpoint));
                this.ProducerRegistry.Reset();
            }

            this.connection.Close();
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
            this.connection.Open(cancellationToken);

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