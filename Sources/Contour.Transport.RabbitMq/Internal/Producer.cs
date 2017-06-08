using System;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Configuration;
using Contour.Sending;
using Contour.Serialization;

using RabbitMQ.Client;

namespace Contour.Transport.RabbitMq.Internal
{
    internal sealed class Producer : IProducer
    {
        private readonly ILog logger;
        private readonly IEndpoint endpoint;
        private readonly IRabbitConnection connection;

        private readonly IPayloadConverterResolver payloadConverterResolver;

        private readonly object syncRoot = new object();
        private CancellationTokenSource cancellationTokenSource;
        private IPublishConfirmationTracker confirmationTracker = new DummyPublishConfirmationTracker();

        public Producer(IEndpoint endpoint, IRabbitConnection connection, MessageLabel label, IRouteResolver routeResolver, bool confirmationIsRequired, IPayloadConverterResolver payloadConverterResolver)
        {
            this.endpoint = endpoint;

            this.connection = connection;
            this.payloadConverterResolver = payloadConverterResolver;
            this.BrokerUrl = connection.ConnectionString;
            this.Label = label;
            this.RouteResolver = routeResolver;
            this.ConfirmationIsRequired = confirmationIsRequired;

            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.BrokerUrl}, {this.Label}, {this.GetHashCode()})");
        }

        public event EventHandler<ProducerStoppedEventArgs> Stopped = (sender, args) => { };

        public bool StopOnChannelShutdown { get; set; }

        /// <summary>
        /// Метка сообщения, которая используется для отправки сообщения.
        /// </summary>
        public MessageLabel Label { get; }

        /// <summary>
        /// A URL assigned to the producer to access the RabbitMQ broker
        /// </summary>
        public string BrokerUrl { get; }

        /// <summary>
        /// Слушатель (получатель) ответных сообщений на запрос.
        /// </summary>
        private IListener CallbackListener { get; set; }

        /// <summary>
        /// Канал подключения к брокеру.
        /// </summary>
        private IRabbitChannel Channel { get; set; }

        /// <summary>
        /// <c>true</c> - если при отправке необходимо подтверждение о том, что брокер сохранил сообщение.
        /// </summary>
        private bool ConfirmationIsRequired { get; }

        /// <summary>
        /// Определитель маршрутов отправки и получения.
        /// </summary>
        private IRouteResolver RouteResolver { get; }

        /// <summary>
        /// Освобождает занятые ресурсы.
        /// </summary>
        public void Dispose()
        {
            this.logger.Trace(m => m("Disposing producer of [{0}].", this.Label));
            this.Stop();
        }

        /// <summary>
        /// Отправляет сообщение <paramref name="message"/> в шину сообщений.
        /// </summary>
        /// <param name="message">
        /// Отправляемое сообщение.
        /// </param>
        /// <returns>
        /// Задача, которая отправляет сообщение в шину сообщений.
        /// </returns>
        public Task Publish(IMessage message)
        {
            if (Monitor.TryEnter(this.syncRoot, 0))
            {
                try
                {
                    var nativeRoute = (RabbitRoute)this.RouteResolver.Resolve(this.endpoint, message.Label);
                    this.logger.Trace(m => m("Emitting message [{0}] through [{1}].", message.Label, nativeRoute));

                    var confirmation = this.confirmationTracker.Track();

                    string contentType = Headers.GetString(message.Headers, Headers.ContentType);
                    IPayloadConverter converter = this.payloadConverterResolver.ResolveConverter(contentType);

                    this.Channel.Publish(nativeRoute, message, converter);
                    return confirmation;
                }
                finally
                {
                    try
                    {
                        Monitor.Exit(this.syncRoot);
                    }
                    catch (Exception)
                    {
                        // Suppress all errors here
                    }
                }
            }

            throw new Exception("Unable to publish via producer because it is not yet started or is recovering");
        }

        /// <summary>
        /// Выполняет запрос в шину сообщений передавая сообщение <see cref="IMessage"/> и ожидает ответ с типом <paramref name="expectedResponseType"/>.
        /// </summary>
        /// <param name="request">
        /// Информация передаваемая как запрос.
        /// </param>
        /// <param name="expectedResponseType">
        /// Ожидаемый тип ответа.
        /// </param>
        /// <returns>
        /// Возвращает задачу, которая выполняет запрос в шину сообщения.
        /// </returns>
        /// <exception cref="Exception">
        /// Исключение генерируемое при возникновении сбоя во время отправки запроса.
        /// </exception>
        /// <exception cref="MessageRejectedException">
        /// Исключение генерируется, если нельзя отправить запрос в шину сообщений.
        /// </exception>
        public Task<IMessage> Request(IMessage request, Type expectedResponseType)
        {
            request.Headers[Headers.ReplyRoute] = this.ResolveCallbackRoute();

            var responseTimeout = Headers.Extract<TimeSpan?>(request.Headers, Headers.Timeout);
            var correlationId = (string)request.Headers[Headers.CorrelationId];

            var responseTask = this.CallbackListener.Expect(correlationId, expectedResponseType, responseTimeout);

            this.Publish(request)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            if (t.Exception != null)
                            {
                                throw t.Exception.Flatten().InnerException;
                            }

                            throw new MessageRejectedException();
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously)
                .Wait();

            return responseTask;
        }

        /// <summary>
        /// Запускает отправитель, после этого можно получать ответы на запросы.
        /// </summary>
        public void Start()
        {
            lock (this.syncRoot)
            {
                this.logger.Trace($"Starting producer [{this.GetHashCode()}] of [{this.Label}]");

                this.cancellationTokenSource = new CancellationTokenSource();
                var token = this.cancellationTokenSource.Token;

                this.Channel = this.connection.OpenChannel(token);
                this.Channel.Shutdown += this.OnChannelShutdown;

                if (this.ConfirmationIsRequired)
                {
                    this.confirmationTracker = new PublishConfirmationTracker(this.Channel);
                    this.Channel.EnablePublishConfirmation();
                    this.Channel.OnConfirmation(this.confirmationTracker.HandleConfirmation);
                }

                this.CallbackListener?.StartConsuming();

                this.logger.Trace($"Producer of [{this.Label}] started successfully");
            }
        }

        /// <summary>
        /// Останавливает отправитель, после этого ответы на запросы не будут обрабатываться.
        /// </summary>
        public void Stop()
        {
            this.InternalStop(OperationStopReason.Regular);
        }

        /// <summary>
        /// Регистрирует слушателя ответа на запрос.
        /// </summary>
        /// <param name="listener">
        /// Слушатель ответа на запрос.
        /// </param>
        /// <exception cref="BusConfigurationException">
        /// Генерируется, если уже зарегистрирован слушатель ответа на запрос.
        /// </exception>
        public void UseCallbackListener(IListener listener)
        {
            if (this.CallbackListener != null)
            {
                throw new BusConfigurationException(
                    "Callback listener for producer [{0}] is already defined.".FormatEx(this.Label));
            }

            this.CallbackListener = listener;
        }

        private void InternalStop(OperationStopReason reason)
        {
            lock (this.syncRoot)
            {
                this.logger.Trace($"Stopping producer of [{this.Label}]");

                this.cancellationTokenSource.Cancel(true);
                this.confirmationTracker?.Dispose();

                try
                {
                    this.Channel.Shutdown -= this.OnChannelShutdown;
                    this.Channel?.Dispose();
                }
                catch (Exception)
                {
                    // Any channel/model disposal exceptions are suppressed
                }
                
                this.Stopped(this, new ProducerStoppedEventArgs(this, reason));
                this.logger.Trace($"Producer of [{this.Label}] stopped successfully");
            }
        }

        /// <summary>
        /// Определяет маршрут ответного сообщения.
        /// </summary>
        /// <returns>
        /// Маршрут ответного сообщения.
        /// </returns>
        /// <exception cref="BusConfigurationException">
        /// Генерируется в случае, если нет конечной точки, которая может отправить сообщение с такой меткой или в случае, если нет ни получателя маршрута ответных сообщений.
        /// </exception>
        private IRoute ResolveCallbackRoute()
        {
            if (this.CallbackListener == null)
            {
                throw new BusConfigurationException($"No reply endpoint is defined for publisher of [{this.Label}].");
            }

            if (this.CallbackListener.Endpoint.CallbackRouteResolver == null)
            {
                throw new BusConfigurationException(
                    $"No callback route resolver is defined for listener on [{this.CallbackListener.Endpoint.ListeningSource}].");
            }

            return this.CallbackListener.Endpoint.CallbackRouteResolver.Resolve(this.endpoint, MessageLabel.Any);
        }

        private void OnChannelShutdown(object sender, ShutdownEventArgs args)
        {
            this.logger.Trace($"Channel shutdown details: {args}");

            if (args.Initiator != ShutdownInitiator.Application)
            {
                if (this.StopOnChannelShutdown)
                {
                    this.logger.Warn("The producer is configured to be stopped on channel failure");
                    this.InternalStop(OperationStopReason.Terminate);
                }
                else
                {
                    this.logger.Warn("The underlying channel has been closed, recovering the producer...");

                    this.Stop();
                    this.Start();
                }
            }
        }
    }
}
