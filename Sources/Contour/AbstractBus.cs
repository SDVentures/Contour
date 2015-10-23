using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Configuration;
using Contour.Receiving;
using Contour.Sending;
using Contour.Serialization;

namespace Contour
{
    /// <summary>
    /// Шина сообщений, которая не знает о транспортном уровне.
    /// </summary>
    internal abstract class AbstractBus : IBus
    {
        /// <summary>
        /// Отслеживает работу компонентов шины сообщений.
        /// </summary>
        private readonly IBusComponentTracker componentTracker = new BusComponentTracker();

        /// <summary>
        /// Конфигурация шины сообщений.
        /// </summary>
        private readonly BusConfiguration configuration;

        /// <summary>
        /// Журнал шины сообщений.
        /// </summary>
        private readonly ILog logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AbstractBus"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация шины сообщений.</param>
        protected AbstractBus(BusConfiguration configuration)
        {
            this.configuration = configuration;

            if (this.configuration.LifecycleHandler != null)
            {
                this.Starting += this.configuration.LifecycleHandler.OnStarting;
                this.Started += this.configuration.LifecycleHandler.OnStarted;
                this.Stopping += this.configuration.LifecycleHandler.OnStopping;
                this.Stopped += this.configuration.LifecycleHandler.OnStopped;
            }
        }

        /// <summary>
        /// Уничтожает экземпляр класса <see cref="AbstractBus"/>. 
        /// </summary>
        ~AbstractBus()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Событие установки соединения.
        /// </summary>
        public event Action<IBus, EventArgs> Connected = (bus, args) => { };

        /// <summary>
        /// Событие разрыва соединения.
        /// </summary>
        public event Action<IBus, EventArgs> Disconnected = (bus, args) => { };

        /// <summary>
        /// Событие запуска шины сообщений.
        /// </summary>
        public event Action<IBus, EventArgs> Started = (bus, args) => { };

        /// <summary>
        /// Событие начала запуска шины сообщений.
        /// </summary>
        public event Action<IBus, EventArgs> Starting = (bus, args) => { };

        /// <summary>
        /// Событие останова шины сообщений.
        /// </summary>
        public event Action<IBus, EventArgs> Stopped = (bus, args) => { };

        /// <summary>
        /// Событие начала останова шины сообщений.
        /// </summary>
        public event Action<IBus, EventArgs> Stopping = (bus, args) => { };

        /// <summary>
        /// Отслеживает работу компонентов шины.
        /// </summary>
        public IBusComponentTracker ComponentTracker
        {
            get
            {
                return this.componentTracker;
            }
        }

        /// <summary>
        /// Конфигурация шины.
        /// </summary>
        public BusConfiguration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        /// <summary>
        /// Конечная точка шины.
        /// </summary>
        public IEndpoint Endpoint
        {
            get
            {
                return this.configuration.Endpoint;
            }
        }

        /// <summary>
        /// Применена ли к шине конфигурация.
        /// </summary>
        public bool IsConfigured { get; internal set; }

        /// <summary>
        /// Готова ли шина к работе.
        /// </summary>
        public bool IsReady
        {
            get
            {
                return this.WhenReady.WaitOne(0);
            }
        }

        /// <summary>
        /// Шина находится в процессе завершения работы.
        /// </summary>
        public bool IsShuttingDown { get; internal set; }

        /// <summary>
        /// Запущена ли шина.
        /// </summary>
        public bool IsStarted { get; internal set; }

        /// <summary>
        /// Обработчик меток сообщений.
        /// </summary>
        public IMessageLabelHandler MessageLabelHandler
        {
            get
            {
                return this.Configuration.MessageLabelHandler;
            }
        }

        /// <summary>
        /// Преобразователь сообщений.
        /// </summary>
        public IPayloadConverter PayloadConverter
        {
            get
            {
                return this.Configuration.Serializer;
            }
        }

        /// <summary>
        /// Получатели сообщений.
        /// </summary>
        public IEnumerable<IReceiver> Receivers
        {
            get
            {
                return this.componentTracker.AllOf<IReceiver>();
            }
        }

        /// <summary>
        /// Отправители сообщений.
        /// </summary>
        public IEnumerable<ISender> Senders
        {
            get
            {
                return this.componentTracker.AllOf<ISender>();
            }
        }

        /// <summary>
        /// Объект синхронизации ожидания готовности шины сообщений.
        /// </summary>
        public abstract WaitHandle WhenReady { get; }

        /// <summary>
        /// Конфигурация шины.
        /// </summary>
        IBusConfiguration IBus.Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        /// <summary>
        /// Проверяет возможность обрабатывать сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns><c>true</c> - если шина сообщения умеет обрабатывать указанную метку сообщений.</returns>
        public bool CanHandle(string label)
        {
            return this.CanHandle(label.ToMessageLabel());
        }

        /// <summary>
        /// Проверяет возможность обрабатывать сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns><c>true</c> - если шина сообщения умеет обрабатывать указанную метку сообщений.</returns>
        public bool CanHandle(MessageLabel label)
        {
            return this.configuration.ReceiverConfigurations.Any(cc => cc.Label.Equals(label));
        }

        /// <summary>
        /// Проверяет возможность построить маршрут для сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns><c>true</c> - если шина сообщения умеет строить маршрут для указанной метки сообщений.</returns>
        public bool CanRoute(string label)
        {
            return this.CanRoute(label.ToMessageLabel());
        }

        /// <summary>
        /// Проверяет возможность построить маршрут для сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns><c>true</c> - если шина сообщения умеет строить маршрут для указанной метки сообщений.</returns>
        public bool CanRoute(MessageLabel label)
        {
            return this.configuration.SenderConfigurations.Any(pc => pc.Label.Equals(label) || (pc.Alias != null && pc.Alias.Equals(label.Name)));
        }

        /// <summary>
        /// Уничтожает шину сообщений.
        /// </summary>
        /// <param name="disposing"><c>true</c> - если нужно освободить ресурсы.</param>
        public virtual void Dispose(bool disposing)
        {
            this.Shutdown();
        }

        /// <summary>
        /// Уничтожает шину сообщений.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Посылает сообщение.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Параметры отправки сообщения.</param>
        /// <typeparam name="T">Тип отправляемого сообщения.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task Emit<T>(string label, T payload, PublishingOptions options = null) where T : class
        {
            return this.Emit(label.ToMessageLabel(), payload, options);
        }

        /// <summary>
        /// Посылает сообщение.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Параметры отправки сообщения.</param>
        /// <typeparam name="T">Тип отправляемого сообщения.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task Emit<T>(MessageLabel label, T payload, PublishingOptions options = null) where T : class
        {
            EnsureCanSendUsing(label);

            return this.InternalSend(label, payload, options);
        }

        /// <summary>
        /// Посылает сообщение.
        /// Метка сообщения вычисляется на основе типа отправляемого сообщения.
        /// </summary>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Параметры отправки сообщения.</param>
        /// <typeparam name="T">Тип отправляемого сообщения.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task Emit<T>(T payload, PublishingOptions options = null) where T : class
        {
            return this.Emit(this.Configuration.MessageLabelResolver.ResolveFrom<T>(), payload, options);
        }

        /// <summary>
        /// Посылает сообщение.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="headers">Заголовки сообщения.</param>
        /// <returns>Задача отправки сообщения.</returns>
        public Task Emit(MessageLabel label, object payload, IDictionary<string, object> headers)
        {
            this.EnsureIsReady();

            return this
                .GetSenderFor(label)
                .Send(label, payload, headers);
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options, Action<TResponse> responseAction) where TResponse : class where TRequest : class
        {
            EnsureCanSendUsing(label);

            this.InternalRequest<TResponse>(label, request, options)
                .ContinueWith(
                    t =>
                        {
                            if (t.IsFaulted)
                            {
                                if (t.Exception != null)
                                {
                                    throw t.Exception;
                                }

                                throw new Exception();
                            }

                            if (t.IsCanceled)
                            {
                                throw new OperationCanceledException();
                            }

                            responseAction(t.Result);
                        })
                .Wait();
        }

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        public void Request<TRequest, TResponse>(MessageLabel label, TRequest request, IDictionary<string, object> headers, Action<TResponse> responseAction) where TRequest : class where TResponse : class
        {
            EnsureCanSendUsing(label);

            this.InternalRequest<TResponse>(label, request, headers)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            if (t.Exception != null)
                            {
                                throw t.Exception;
                            }

                            throw new Exception();
                        }

                        if (t.IsCanceled)
                        {
                            throw new OperationCanceledException();
                        }

                        responseAction(t.Result);
                    })
                .Wait();
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Метка сообщения вычисляется на основе типа запроса.
        /// </summary>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(TRequest request, RequestOptions options, Action<TResponse> responseAction) where TRequest : class where TResponse : class
        {
            Request(this.Configuration.MessageLabelResolver.ResolveFrom<TRequest>(), request, options, responseAction);
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Метка сообщения вычисляется на основе типа запроса.
        /// </summary>
        /// <param name="request">Тело запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(TRequest request, Action<TResponse> responseAction) where TRequest : class where TResponse : class
        {
            this.RequestAsync<TRequest, TResponse>(this.Configuration.MessageLabelResolver.ResolveFrom<TRequest>(), request);
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(string label, TRequest request, RequestOptions options, Action<TResponse> responseAction) where TRequest : class where TResponse : class
        {
            Request(label.ToMessageLabel(), request, options, responseAction);
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(MessageLabel label, TRequest request, Action<TResponse> responseAction) where TRequest : class where TResponse : class
        {
            Request(label, request, (RequestOptions)null, responseAction);
        }

        /// <summary>
        /// Посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="responseAction">Действие вызываемое при получении ответа.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        public void Request<TRequest, TResponse>(string label, TRequest request, Action<TResponse> responseAction) where TResponse : class where TRequest : class
        {
            Request(label.ToMessageLabel(), request, responseAction);
        }

        /// <summary>
        /// Асинхронно посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options = null) where TResponse : class where TRequest : class
        {
            EnsureCanSendUsing(label);

            return this.InternalRequest<TResponse>(label, request, options);
        }

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(MessageLabel label, TRequest request, IDictionary<string, object> headers) where TRequest : class where TResponse : class
        {
            EnsureCanSendUsing(label);

            return this.InternalRequest<TResponse>(label, request, headers);
        }

        /// <summary>
        /// Асинхронно посылает сообщение в формате запрос-ответ.
        /// Метка сообщения вычисляется на основе типа запроса.
        /// </summary>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, RequestOptions options = null) where TRequest : class where TResponse : class
        {
            return this.RequestAsync<TRequest, TResponse>(this.Configuration.MessageLabelResolver.ResolveFrom<TRequest>(), request, options);
        }

        /// <summary>
        /// Асинхронно посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="request">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <typeparam name="TRequest">Тип запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        public Task<TResponse> RequestAsync<TRequest, TResponse>(string label, TRequest request, RequestOptions options = null) where TResponse : class where TRequest : class
        {
            return this.RequestAsync<TRequest, TResponse>(label.ToMessageLabel(), request, options);
        }

        /// <summary>
        /// Завершает работу шины сообщений.
        /// </summary>
        public abstract void Shutdown();

        /// <summary>
        /// Запускает шину сообщений.
        /// </summary>
        /// <param name="waitForReadiness">Если <c>true</c> - тогда необходимо дождаться готовности шины сообщений.</param>
        public abstract void Start(bool waitForReadiness = true);

        /// <summary>
        /// Останавливает шину сообщений.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Вычисляет отправителя для указанной метки сообщений.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <returns>Отправитель сообщения с указанной меткой.</returns>
        /// <exception cref="BusConfigurationException">Генерируется исключение, если нет отправителя для указанной метки.</exception>
        protected ISender GetSenderFor(MessageLabel label)
        {
            ISender sender = this.Senders.FirstOrDefault(s => s.CanRoute(label)) ?? this.Senders.FirstOrDefault(s => s.CanRoute(MessageLabel.Any));

            if (sender == null)
            {
                throw new BusConfigurationException("No sender is configured to send [{0}].".FormatEx(label));
            }

            return sender;
        }

        /// <summary>
        /// Асинхронно посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело запроса.</param>
        /// <param name="options">Параметры отправки запроса.</param>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        protected Task<TResponse> InternalRequest<TResponse>(MessageLabel label, object payload, RequestOptions options) where TResponse : class
        {
            this.EnsureIsReady();

            return this.GetSenderFor(label)
                    .Request<TResponse>(label, payload, options ?? new RequestOptions());
        }

        /// <summary>
        /// Асинхронно посылает сообщение в формате запрос-ответ.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело запроса.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <typeparam name="TResponse">Тип ответа.</typeparam>
        /// <returns>Задача отправки сообщения.</returns>
        protected Task<TResponse> InternalRequest<TResponse>(MessageLabel label, object payload, IDictionary<string, object> headers) where TResponse : class
        {
            this.EnsureIsReady();

            return this.GetSenderFor(label)
                    .Request<TResponse>(label, payload, headers ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Асинхронно посылает сообщение.
        /// Для отправки необходимо указать метку сообщения, на основе которой строится маршрут сообщения.
        /// </summary>
        /// <param name="label">Метка посылаемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Параметры отправки сообщения.</param>
        /// <returns>Задача отправки сообщения.</returns>
        protected Task InternalSend(MessageLabel label, object payload, PublishingOptions options)
        {
            this.EnsureIsReady();

            return this.GetSenderFor(label)
                    .Send(label, payload, options ?? new PublishingOptions());
        }

        /// <summary>
        /// Генерирует событие об установке соединения.
        /// </summary>
        protected virtual void OnConnected()
        {
            this.Connected(this, null);
        }

        /// <summary>
        /// Генерирует событие о разрыве соединения.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            this.Disconnected(this, null);
        }

        /// <summary>
        /// Генерирует событие о запуске шины сообщений.
        /// </summary>
        protected virtual void OnStarted()
        {
            this.Started(this, null);

            this.logger.InfoFormat(
                "Started [{0}] with endpoint [{1}].".FormatEx(
                    this.GetType().Name, 
                    this.Endpoint));
        }

        /// <summary>
        /// Генерирует событие о начале запуска шины сообщений.
        /// </summary>
        protected virtual void OnStarting()
        {
            this.logger.InfoFormat(
                "Starting [{0}] with endpoint [{1}].".FormatEx(
                    this.GetType().Name, 
                    this.Endpoint));

            this.Starting(this, null);
        }

        /// <summary>
        /// Генерирует событие об остановке шины сообщений.
        /// </summary>
        protected virtual void OnStopped()
        {
            this.Stopped(this, null);

            this.logger.InfoFormat(
                "Stopped [{0}] with endpoint [{1}].".FormatEx(
                    this.GetType().Name, 
                    this.Endpoint));
        }

        /// <summary>
        /// Генерирует событие о начале остановки шины сообщений.
        /// </summary>
        protected virtual void OnStopping()
        {
            this.logger.InfoFormat(
                "Stopping [{0}] with endpoint [{1}].".FormatEx(
                    this.GetType().Name, 
                    this.Endpoint));

            this.IsStarted = false;

            this.Stopping(this, null);
        }

        /// <summary>
        /// Останавливает и заново запускает шину сообщений.
        /// </summary>
        /// <param name="waitForReadiness"><c>true</c> - если нужно дождаться готовности шины.</param>
        protected abstract void Restart(bool waitForReadiness = true);

        /// <summary>
        /// Проверяет возможность отправить сообщение с указанной меткой.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <exception cref="InvalidOperationException">Генерируется если нельзя отправлять указанные метки сообщения.</exception>
        // ReSharper disable UnusedParameter.Local
        private static void EnsureCanSendUsing(MessageLabel label)
            // ReSharper restore UnusedParameter.Local
        {
            if (label.IsAny)
            {
                throw new InvalidOperationException("Can't send using Any label.");
            }

            if (label.IsEmpty)
            {
                throw new InvalidOperationException("Can't send using Empty label.");
            }
        }

        /// <summary>
        /// Проверяет, что шина сообщений готова к работе.
        /// </summary>
        /// <exception cref="BusNotReadyException">Если шина не готова к работе, генерируется исключение.</exception>
        private void EnsureIsReady()
        {
            if (!this.WhenReady.WaitOne(TimeSpan.FromSeconds(5)))
            {
                throw new BusNotReadyException();
            }
        }
    }
}
