﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Contour.Configuration;
using Contour.Helpers;
using Contour.Receiving;
using Contour.Sending;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// The rabbit bus.
    /// </summary>
    internal class RabbitBus : AbstractBus, IBusAdvanced
    {
        private readonly object syncRoot = new object();
        private readonly ILog logger = LogManager.GetLogger<RabbitBus>();
        private readonly ManualResetEvent isRestarting = new ManualResetEvent(false);
        private readonly ManualResetEvent ready = new ManualResetEvent(false);
        private readonly IConnectionPool<IRabbitConnection> connectionPool;

        private CancellationTokenSource cancellationTokenSource;
        private Task restartTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitBus" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RabbitBus(BusConfiguration configuration)
            : base(configuration)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var completion = new TaskCompletionSource<object>();
            completion.SetResult(new object());
            this.restartTask = completion.Task;
            
            this.connectionPool = new RabbitConnectionPool(this);
        }
        
        /// <summary>
        /// Gets the when ready.
        /// </summary>
        public override WaitHandle WhenReady => this.ready;

        /// <summary>
        /// The panic.
        /// </summary>
        public void Panic()
        {
            
            // TODO никто не вызывает, может стоит удалить метод
            this.Restart();
        }

        /// <summary>
        /// Starts a bus
        /// </summary>
        /// <param name="waitForReadiness">The wait for readiness.</param>
        /// <exception cref="AggregateException">Any exceptions thrown during the bus start</exception>
        public override void Start(bool waitForReadiness = true)
        {
            if (this.IsStarted || this.IsShuttingDown)
            {
                return;
            }

            this.Restart(waitForReadiness);
        }

        public override void Stop()
        {
            this.ResetRestartTask();

            var token = this.cancellationTokenSource.Token;
            this.restartTask = Task.Factory.StartNew(this.StopTask, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            this.restartTask.Wait(5000);
        }

        /// <summary>
        /// Shuts the bus down
        /// </summary>
        public override void Shutdown()
        {
            this.logger.InfoFormat(
                "Shutting down [{0}] with endpoint [{1}].", 
                this.GetType().Name, 
                this.Endpoint);

            this.IsShuttingDown = true;
            this.connectionPool.Drop();

            this.Stop();

            this.logger.Trace(m => m("{0}: resetting bus configuration", this.Endpoint));
            this.IsConfigured = false;

            // если не ожидать завершения задачи до сброса флага IsShuttingDown,
            // тогда в случае ошибок (например, когда обработчик пытается отправить сообщение в шину, а она в состоянии закрытия)
            // задача может не успеть закрыться и она входит в бесконечное ожидание в методе Restart -> ResetRestartTask.
            this.restartTask.Wait();

            this.ComponentTracker.UnregisterAll();
            this.IsShuttingDown = false;
        }

        /// <summary>
        /// Registers a receiver using the <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">
        /// Receiver configuration
        /// </param>
        /// <param name="isCallback">
        /// Denotes if a receiver should handle the callback messages
        /// </param>
        /// <returns>
        /// The <see cref="RabbitReceiver"/>.
        /// </returns>
        public RabbitReceiver RegisterReceiver(IReceiverConfiguration configuration, bool isCallback = false)
        {
            this.logger.Trace(
                $"Registering a new receiver of [{configuration.Label}] with connection string [{configuration.Options.GetConnectionString()}]");

            RabbitReceiver receiver;
            if (isCallback)
            {
                receiver = new RabbitCallbackReceiver(this, configuration, this.connectionPool);

                // No need to subscribe to listener-created event as it will not be fired by the callback receiver. A callback listener is not checked with listeners in other receivers for compatibility.
            }
            else
            {
                receiver = new RabbitReceiver(this, configuration, this.connectionPool);
                receiver.ListenerCreated += this.OnListenerCreated;
            }

            this.ComponentTracker.Register(receiver);

            this.logger.Trace(
                $"A receiver of [{configuration.Label}] with connection string [{configuration.Options.GetConnectionString()}] registered successfully");
            return receiver;
        }

        /// <summary>
        /// Registers a sender using <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">
        /// Sender configuration
        /// </param>
        /// <returns>
        /// The <see cref="RabbitSender"/>.
        /// </returns>
        public RabbitSender RegisterSender(ISenderConfiguration configuration)
        {
            this.logger.Trace(
                $"Registering a new sender of [{configuration.Label}] with connection string [{configuration.Options.GetConnectionString()}]");

            var sender = new RabbitSender(this, configuration, this.connectionPool, this.Configuration.Filters.ToList());
            this.ComponentTracker.Register(sender);

            this.logger.Trace(
                $"A sender of [{configuration.Label}] with connection string [{configuration.Options.GetConnectionString()}] registered successfully");

            return sender;
        }

        protected override void Restart(bool waitForReadiness = true)
        {
            lock (this.syncRoot)
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
            this.restartTask = Task.Factory.StartNew(this.StopTask, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(_ => this.StartTask(), token, TaskContinuationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(
                    t =>
                        {
                            this.isRestarting.Reset();
                            if (t.IsFaulted)
                            {   
                                // TODO ошибка тут приводит к тому, что сервис зависает навсегда с неготовой шиной
                                // если раньше это приводило к потере сообщений, при попытке их обработать, теперь сервис просто ничего делать не будет
                                this.logger.Error(m => m("Error on restarting bus: {0}", this.Endpoint.Address), t.Exception.InnerException);
                                throw t.Exception.InnerException;
                            }
                        });

            if (waitForReadiness)
            {
                // TODO почему ждем 5 секунд, если не дождались, то можем получить неготовую шину до перезапуска
                this.restartTask.Wait(5000);
            }
        }

        private void StartTask()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            this.OnStarting();

            this.logger.Trace(m => m("{0}: configuring.", this.Endpoint));
            this.Configure();

            this.logger.Trace(m => m("{0}: starting components.", this.Endpoint));
            this.ComponentTracker.StartAll();

            this.logger.Trace(m => m("{0}: marking as ready.", this.Endpoint));
            this.IsStarted = true;
            this.ready.Set();

            this.OnStarted();
        }

        private void StopTask()
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
            
            this.OnStopped();
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

        private void Configure()
        {
            if (this.IsConfigured)
            {
                return;
            }

            var name = this.GetType().Name;
            var senderConfigurations = this.Configuration.SenderConfigurations.ToList();
            var receiverConfigurations = this.Configuration.ReceiverConfigurations.ToList();

            this.logger.Trace(
                $"Configuring [{name}] with endpoint [{this.Endpoint}]:\r\nSenders:\r\n\t{string.Join("\r\n\t", senderConfigurations.Select(s => $"[{s.Label}]\t=>\t{s.Options.GetConnectionString()}"))}\r\nReceivers:\r\n\t{string.Join("\r\n\t", receiverConfigurations.Select(r => $"[{r.Label}]\t=>\t{r.Options.GetConnectionString()}"))}");

            foreach (var sender in senderConfigurations)
            {
                this.RegisterSender(sender);
            }

            foreach (var receiver in receiverConfigurations)
            {
                this.RegisterReceiver(receiver);
            }

            this.IsConfigured = true;
            this.logger.Info($"Configuration of [{name}] completed successfully");
        }
        
        private void OnListenerCreated(object sender, ListenerCreatedEventArgs e)
        {
            this.Receivers
                .Where(r => r is RabbitReceiver)
                .Cast<RabbitReceiver>()
                .ForEach(r => r.CheckIfCompatible(e.Listener));
        }
    }
}