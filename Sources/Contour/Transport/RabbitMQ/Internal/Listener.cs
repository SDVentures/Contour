using RabbitMQ.Client;

namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using Helpers;
    using Helpers.Timing;
    using Receiving;
    using Receiving.Consumers;
    using Validation;
    using global::RabbitMQ.Client.Events;

    /// <summary>
    /// Слушатель канала.
    /// </summary>
    internal sealed class Listener : IListener
    {
        /// <summary>
        /// Обработчики сообщений.
        /// Каждому обработчику соответствует своя метка сообщения.
        /// </summary>
        private readonly IDictionary<MessageLabel, ConsumingAction> consumers = new ConcurrentDictionary<MessageLabel, ConsumingAction>();

        /// <summary>
        /// Порт канала, на который приходит сообщение.
        /// </summary>
        private readonly ISubscriptionEndpoint endpoint;

        /// <summary>
        /// Ожидания ответных сообщений на запрос.
        /// </summary>
        private readonly ConcurrentDictionary<string, Expectation> expectations = new ConcurrentDictionary<string, Expectation>();

        /// <summary>
        /// Хранилище заголовков входящего сообщения.
        /// </summary>
        private readonly IIncomingMessageHeaderStorage messageHeaderStorage;

        /// <summary>
        /// Объект синхронизации.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// Журнал работы.
        /// </summary>
        private readonly ILog logger;

        /// <summary>
        /// Реестр механизмов проверки сообщений.
        /// </summary>
        private readonly MessageValidatorRegistry validatorRegistry;
        private readonly IBusContext busContext;
        private readonly IRabbitConnection connection;
        
        private readonly ConcurrentBag<RabbitChannel> channels = new ConcurrentBag<RabbitChannel>();

        private readonly ConcurrentDictionary<string, string[]> exchangeMetricTags = new ConcurrentDictionary<string, string[]>();

        private readonly string[] responseMetricTags;

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Признак: слушатель потребляет сообщения.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private bool isConsuming;

        /// <summary>
        /// Таймер, который отслеживает, что время ожидания ответа на запрос вышло.
        /// </summary>
        private ITicketTimer ticketTimer;
        private ConcurrentBag<Task> workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Listener"/> class. 
        /// </summary>
        /// <param name="busContext">
        /// The bus Context.
        /// </param>
        /// <param name="connection">
        /// Соединение с шиной сообщений
        /// </param>
        /// <param name="endpoint">
        /// Прослушиваемый порт.
        /// </param>
        /// <param name="receiverOptions">
        /// Настройки получателя.
        /// </param>
        /// <param name="validatorRegistry">
        /// Реестр механизмов проверки сообщений.
        /// </param>
        public Listener(IBusContext busContext, IRabbitConnection connection, ISubscriptionEndpoint endpoint, RabbitReceiverOptions receiverOptions, MessageValidatorRegistry validatorRegistry)
        {
            this.busContext = busContext;
            this.connection = connection;
            
            this.endpoint = endpoint;
            this.validatorRegistry = validatorRegistry;

            this.ReceiverOptions = receiverOptions;
            this.BrokerUrl = connection.ConnectionString;

            this.messageHeaderStorage = this.ReceiverOptions.GetIncomingMessageHeaderStorage().Value;

            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.BrokerUrl}, {this.GetHashCode()})");

            this.responseMetricTags = new[] { "deliveryEndpoint:" + this.busContext.Endpoint.Address };
        }

        public event EventHandler<ListenerStoppedEventArgs> Stopped = (sender, args) => { };

        /// <summary>
        /// Тип обработчика сообщений.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение.
        /// </param>
        internal delegate void ConsumingAction(RabbitDelivery delivery);
        
        /// <summary>
        /// Метки сообщений, которые может обработать слушатель.
        /// </summary>
        public IEnumerable<MessageLabel> AcceptedLabels => this.consumers.Keys;

        /// <summary>
        /// Порт канала, который прослушивается на входящие сообщения.
        /// </summary>
        public ISubscriptionEndpoint Endpoint => this.endpoint;

        /// <summary>
        /// Настройки получателя.
        /// </summary>
        public RabbitReceiverOptions ReceiverOptions { get; }

        /// <summary>
        /// A URL assigned to the listener to access the RabbitMQ broker
        /// </summary>
        public string BrokerUrl { get; }

        /// <summary>
        /// Denotes if a listener should dispose itself if any channel related errors have occurred
        /// </summary>
        public bool StopOnChannelShutdown { get; set; }

        public IMetricsCollector MetricsCollector { get; set; }

        /// <summary>
        /// Создает входящее сообщение.
        /// </summary>
        /// <param name="deliveryChannel">
        /// The delivery Channel.
        /// </param>
        /// <param name="args">
        /// Аргументы, с которыми получено сообщение.
        /// </param>
        /// <returns>
        /// Входящее сообщение.
        /// </returns>
        public RabbitDelivery BuildDeliveryFrom(RabbitChannel deliveryChannel, BasicDeliverEventArgs args)
        {
            return new RabbitDelivery(this.busContext, deliveryChannel, args, this.ReceiverOptions.IsAcceptRequired());
        }

        /// <summary>
        /// Освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            this.StopConsuming();
        }

        /// <summary>
        /// Возвращает задачу, с ожиданием ответа на запрос.
        /// </summary>
        /// <param name="correlationId">
        /// Корреляционный идентификатор, с помощью которого определяется принадлежность ответа определенному запросу.
        /// </param>
        /// <param name="expectedResponseType">
        /// Ожидаемый тип ответа.
        /// </param>
        /// <param name="timeout">
        /// Время ожидания ответа.
        /// </param>
        /// <returns>
        /// Задача ожидания ответа на запрос.
        /// </returns>
        public Task<IMessage> Expect(string correlationId, Type expectedResponseType, TimeSpan? timeout)
        {
            Expectation expectation;
            if (this.expectations.TryGetValue(correlationId, out expectation))
            {
                return expectation.Task;
            }

            // Используется блокировка, чтобы гарантировать что метод CreateExpectation будет вызван один раз, т.к. это не гарантируется реализацией метода GetOrAdd.
            lock (this.syncRoot)
            {
                expectation = this.expectations.GetOrAdd(correlationId, c => this.CreateExpectation(c, expectedResponseType, timeout));
                return expectation.Task;
            }
        }

        /// <summary>
        /// Добавляет еще одного обработчика сообщений.
        /// </summary>
        /// <param name="label">
        /// Метка сообщения, которое может быть обработано.
        /// </param>
        /// <param name="consumer">
        /// Обработчик сообщения.
        /// </param>
        /// <param name="validator">
        /// Механизм проверки входящего сообщения.
        /// </param>
        /// <typeparam name="T">
        /// Тип входящего сообщения.
        /// </typeparam>
        public void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer, IMessageValidator validator) where T : class
        {
            ConsumingAction consumingAction = delivery =>
                {
                    IConsumingContext<T> context = delivery.BuildConsumingContext<T>(label);

                    if (validator != null)
                    {
                        validator.Validate(context.Message).ThrowIfBroken();
                    }
                    else
                    {
                        this.validatorRegistry.Validate(context.Message);
                    }

                    var sw = Stopwatch.StartNew();

                    consumer.Handle(context);

                    this.MetricsCollector?.Histogram("contour.rmq.consuming.duration", sw.ElapsedMilliseconds, 1D, new[] { "endpoint:" + this.busContext.Endpoint.Address, "label:" + label.ToString() });
                };

            this.consumers[label] = consumingAction;
        }

        /// <summary>
        /// Запускает обработку входящих сообщений.
        /// </summary>
        public void StartConsuming()
        {
            lock (this.syncRoot)
            {
                if (this.isConsuming)
                {
                    return;
                }

                this.logger.InfoFormat("Starting consuming on [{0}].", this.endpoint.ListeningSource);

                this.cancellationTokenSource = new CancellationTokenSource();
                this.ticketTimer = new RoughTicketTimer(TimeSpan.FromSeconds(1));

                var count = (int)this.ReceiverOptions.GetParallelismLevel().Value;
                var token = this.cancellationTokenSource.Token;

                // In order to increase the performance of the listener on startup the task factory should be configured to create long running tasks. These tasks will run on dedicated threads instead of thread pool threads which may be created with delays in certain conditions. As a workaround for this condition one can set the minimum number of threads created by the pool before switching the thread creation policy. See https://stackoverflow.com/questions/22036365/newly-created-threads-using-task-factory-startnew-starts-very-slowly for details.

                this.workers = new ConcurrentBag<Task>(Enumerable
                    .Range(0, count)
                    .Select(
                        _ =>
                            Task.Factory.StartNew(
                                () => this.ConsumerTaskMethod(token), token, TaskCreationOptions.LongRunning,
                                TaskScheduler.Default)));

                this.isConsuming = true;
                this.logger.Trace("Listener started successfully");
            }
        }

        /// <summary>
        /// Останавливает обработку входящих сообщений.
        /// </summary>
        public void StopConsuming()
        {
            this.InternalStop(OperationStopReason.Regular);
        }

        /// <summary>
        /// Проверяет поддерживает слушатель обработку сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">
        /// Метка сообщения.
        /// </param>
        /// <returns>
        /// Если <c>true</c> - слушатель поддерживает обработку сообщений, иначе - <c>false</c>.
        /// </returns>
        public bool Supports(MessageLabel label)
        {
            return this.AcceptedLabels.Contains(label);
        }

        /// <summary>
        /// Доставляет сообщение до обработчика.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение.
        /// </param>
        private void Deliver(RabbitDelivery delivery)
        {
            this.logger.Trace(m => m("Received delivery labeled [{0}] from exchange [{1}] with consumer [{2}].", delivery.Label, delivery.Args.Exchange, delivery.Args.ConsumerTag));

            if (delivery.Headers.ContainsKey(Headers.OriginalMessageId))
            {
                this.logger.Trace(m => m("Сквозной идентификатор сообщения [{0}].", Headers.GetString(delivery.Headers, Headers.OriginalMessageId)));
            }

            if (delivery.Headers.ContainsKey(Headers.Breadcrumbs))
            {
                this.logger.Trace(m => m("Сообщение было обработано в конечных точках: [{0}].", Headers.GetString(delivery.Headers, Headers.Breadcrumbs)));
            }

            var tags = delivery.IsResponse ? this.responseMetricTags : this.exchangeMetricTags.GetOrAdd(
                delivery.Args.Exchange, 
                e => new[]
                        {
                            "endpoint:" + this.busContext.Endpoint.Address,
                            "label:" + e
                        });

            this.MetricsCollector?.Increment("contour.rmq.consuming.count", 1D, tags);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                bool processed = this.TryHandleAsResponse(delivery);

                if (!processed)
                {
                    processed = this.TryHandleAsSubscription(delivery);
                }

                if (!processed)
                {
                    this.MetricsCollector?.Increment("contour.rmq.unhandleddelivery.count", 1D, tags);
                    this.OnUnhandled(delivery);
                }
            }
            catch (Exception ex)
            {
                this.MetricsCollector?.Increment("contour.rmq.faileddelivery.count", 1D, tags);
                this.OnFailure(delivery, ex);
            }

            stopwatch.Stop();
            this.MetricsCollector?.Increment("contour.rmq.consumed.count", 1D, tags);
            this.MetricsCollector?.Decrement("contour.rmq.consuming.count", 1D, tags);
            this.logger.Trace(m => m("Message labeled [{0}] processed in {1} ms.", delivery.Label, stopwatch.ElapsedMilliseconds));
        }

        private void InternalStop(OperationStopReason reason)
        {
            lock (this.syncRoot)
            {
                if (!this.isConsuming)
                {
                    return;
                }

                this.isConsuming = false;
                this.logger.InfoFormat("Stopping consuming on [{0}].", this.endpoint.ListeningSource);

                this.cancellationTokenSource.Cancel(true);

                Task worker;
                while (this.workers.TryTake(out worker))
                {
                    try
                    {
                        worker.Wait();
                        worker.Dispose();
                    }
                    catch (Exception)
                    {
                        // May catch OperationCanceledExceptions here on Task.Wait()
                    }
                }

                RabbitChannel channel;
                while (this.channels.TryTake(out channel))
                {
                    try
                    {
                        channel.Shutdown -= this.OnChannelShutdown;
                        channel.Dispose();
                    }
                    catch (Exception)
                    {
                        // Any channel/model disposal exceptions are suppressed
                    }
                }

                this.ticketTimer.Dispose();
                this.expectations.Values.ForEach(e => e.Cancel());

                this.Stopped(this, new ListenerStoppedEventArgs(this, reason));
                this.logger.Trace("Listener stopped successfully");
            }
        }


        /// <summary>
        /// Формирует ответ на запрос.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение, которое является ответом.
        /// </param>
        /// <param name="responseType">
        /// Тип ответного сообщения.
        /// </param>
        /// <returns>
        /// Сообщение с ответом на запрос.
        /// </returns>
        private IMessage BuildResponse(IDelivery delivery, Type responseType)
        {
            IMessage response = delivery.UnpackAs(responseType);

            this.validatorRegistry.Validate(response);

            return response;
        }

        /// <summary>
        /// Обрабатывает сообщение.
        /// </summary>
        /// <param name="token">
        /// Сигнальный объект аварийного досрочного завершения обработки.
        /// </param>
        private void ConsumerTaskMethod(CancellationToken token)
        {
            try
            {
                RabbitChannel channel;
                var consumer = this.InitializeConsumer(token, out channel);

                while (!token.IsCancellationRequested)
                {
                    var message = consumer.Dequeue();
                    this.Deliver(this.BuildDeliveryFrom(channel, message));
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"Consume operation of listener has been canceled");
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    $"Listener of [{this.endpoint.ListeningSource}] at [{this.BrokerUrl}] has failed due to [{ex.Message}]");
            }
        }

        /// <summary>
        /// Создаёт ожидание ответа на запрос.
        /// </summary>
        /// <param name="correlationId">Корреляционный идентификатор, с помощью которого определяется принадлежность ответа определенному запросу.</param>
        /// <param name="expectedResponseType">Ожидаемый тип ответа.</param>
        /// <param name="timeout">Время ожидания ответа.</param>
        /// <returns>Ожидание ответа на запрос.</returns>
        private Expectation CreateExpectation(string correlationId, Type expectedResponseType, TimeSpan? timeout)
        {
            long? timeoutTicket = null;
            if (timeout.HasValue)
            {
                timeoutTicket = this.ticketTimer.Acquire(
                    timeout.Value,
                    () =>
                    {
                        this.OnResponseTimeout(correlationId);
                    });
            }

            return new Expectation(d => this.BuildResponse(d, expectedResponseType), timeoutTicket);
        }
        
        private CancellableQueueingConsumer InitializeConsumer(CancellationToken token, out RabbitChannel channel)
        {
            // Opening a new channel may lead to a new connection creation
            channel = this.connection.OpenChannel(token);
            channel.Shutdown += this.OnChannelShutdown;
            this.channels.Add(channel);
            
            if (this.ReceiverOptions.GetQoS().HasValue)
            {
                channel.SetQos(
                    this.ReceiverOptions.GetQoS().Value);
            }

            var consumer = channel.BuildCancellableConsumer(token);
            var tag = channel.StartConsuming(
                this.endpoint.ListeningSource,
                this.ReceiverOptions.IsAcceptRequired(),
                consumer);

            this.logger.Trace(
                $"A consumer tagged [{tag}] has been registered in listener of [{string.Join(",", this.AcceptedLabels)}]");

            return consumer;
        }

        private void OnChannelShutdown(IChannel channel, ShutdownEventArgs args)
        {
            this.logger.Trace($"Channel shutdown details: {args}");

            if (args.Initiator != ShutdownInitiator.Application)
            {
                if (this.StopOnChannelShutdown)
                {
                    this.logger.Warn("The listener is configured to be stopped on channel failure");
                    this.InternalStop(OperationStopReason.Terminate);
                }
                else
                {
                    this.logger.Warn("The underlying channel has been closed, recovering the listener...");

                    this.StopConsuming();
                    this.StartConsuming();
                }
            }
        }

        /// <summary>
        /// Обработчик события о сбое обработки сообщения.
        /// </summary>
        /// <param name="delivery">
        /// Сообщение, обработка которого привела к сбою.
        /// </param>
        /// <param name="exception">
        /// Исключение сгенерированное во время сбоя.
        /// </param>
        private void OnFailure(RabbitDelivery delivery, Exception exception)
        {
            this.logger.Warn(m => m("Failed to process message labeled [{0}] on queue [{1}].", delivery.Label, this.endpoint.ListeningSource), exception);

            this.ReceiverOptions.GetFailedDeliveryStrategy()
                .Value.Handle(new RabbitFailedConsumingContext(delivery, exception));
        }

        /// <summary>
        /// Обрабатывает превышение времени ожидания ответа.
        /// </summary>
        /// <param name="correlationId">Корреляционный идентификатор, с помощью которого определяется принадлежность ответа определенному запросу.</param>
        private void OnResponseTimeout(string correlationId)
        {
            Expectation expectation;
            if (this.expectations.TryRemove(correlationId, out expectation))
            {
                expectation.Timeout();
            }
        }

        /// <summary>
        /// Обработчик события о ненайденном обработчике сообщений.
        /// </summary>
        /// <param name="delivery">
        /// Сообщение, для которого не найден обработчик.
        /// </param>
        private void OnUnhandled(RabbitDelivery delivery)
        {
            this.logger.Warn(m => m("No handler for message labeled [{0}] on queue [{1}].", delivery.Label, this.endpoint.ListeningSource));

            this.ReceiverOptions.GetUnhandledDeliveryStrategy()
                .Value.Handle(new RabbitUnhandledConsumingContext(delivery));
        }

        /// <summary>
        /// Пытается обработать сообщение как запрос.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение.
        /// </param>
        /// <returns>
        /// Если <c>true</c> - тогда сообщение обработано как запрос, иначе - <c>false</c>.
        /// </returns>
        private bool TryHandleAsResponse(RabbitDelivery delivery)
        {
            if (!delivery.IsResponse)
            {
                return false;
            }

            string correlationId = delivery.CorrelationId;

            Expectation expectation;
            if (!this.expectations.TryRemove(correlationId, out expectation))
            {
                return false;
            }

            if (expectation.TimeoutTicket.HasValue)
            {
                this.ticketTimer.Cancel(expectation.TimeoutTicket.Value);
            }

            expectation.Complete(delivery);
            return true;
        }

        /// <summary>
        /// Пытается обработать сообщение как одностороннее.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение.
        /// </param>
        /// <returns>
        /// Если <c>true</c> - входящее сообщение обработано, иначе <c>false</c>.
        /// </returns>
        private bool TryHandleAsSubscription(RabbitDelivery delivery)
        {
            ConsumingAction consumingAction;
            if (!this.consumers.TryGetValue(delivery.Label, out consumingAction))
            {
                this.consumers.TryGetValue(MessageLabel.Any, out consumingAction);

                if (consumingAction == null)
                {
                    consumingAction = this.consumers.Values.FirstOrDefault();
                }
            }

            if (consumingAction != null)
            {
                this.messageHeaderStorage.Store(delivery.Headers);
                consumingAction(delivery);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ожидание ответа на запрос.
        /// </summary>
        internal class Expectation
        {
            /// <summary>
            /// Источник сигнальных объектов об аварийном завершении задачи.
            /// </summary>
            private readonly TaskCompletionSource<IMessage> completionSource;

            /// <summary>
            /// Построитель ответа.
            /// </summary>
            private readonly Func<IDelivery, IMessage> responseBuilderFunc;

            /// <summary>
            /// Секундомер для замера длительности ожидания ответа.
            /// </summary>
            private readonly Stopwatch completionStopwatch;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Expectation"/>.
            /// </summary>
            /// <param name="responseBuilderFunc">
            /// Построитель ответа.
            /// </param>
            /// <param name="timeoutTicket">
            /// Квиток об учете времени ожидания ответа.
            /// </param>
            /// <param name="metricsCollector">Metrics collector</param>
            /// <param name="endpointName">Endpoint name</param>
            public Expectation(Func<IDelivery, IMessage> responseBuilderFunc, long? timeoutTicket)
            {
                this.responseBuilderFunc = responseBuilderFunc;
                this.TimeoutTicket = timeoutTicket;
                this.completionSource = new TaskCompletionSource<IMessage>();
                this.completionStopwatch = Stopwatch.StartNew();
            }

            /// <summary>
            /// Задача завершения ожидания.
            /// </summary>
            public Task<IMessage> Task
            {
                get
                {
                    return this.completionSource.Task;
                }
            }

            /// <summary>
            /// Квиток об учете времени ожидания ответа.
            /// </summary>
            public long? TimeoutTicket { get; private set; }

            /// <summary>
            /// Отменяет ожидание ответа.
            /// </summary>
            public void Cancel()
            {
                this.completionSource.TrySetException(new OperationCanceledException());
            }

            /// <summary>
            /// Выполняет обработку ответа на запрос.
            /// </summary>
            /// <param name="delivery">
            /// Входящее сообщение - ответ на запрос.
            /// </param>
            public void Complete(RabbitDelivery delivery)
            {
                try
                {
                    this.completionStopwatch.Stop();

                    IMessage response = this.responseBuilderFunc(delivery);
                    this.completionSource.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    this.completionSource.TrySetException(ex);
                    throw;
                }
            }

            /// <summary>
            /// Устанавливает, что при ожидании вышло время, за которое должен был быть получен ответ.ё
            /// </summary>
            public void Timeout()
            {
                this.completionSource.TrySetException(new TimeoutException());
            }
        }
    }
}
