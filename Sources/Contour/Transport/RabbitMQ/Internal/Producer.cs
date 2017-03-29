using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Contour.Configuration;
using Contour.Sending;
using RabbitMQ.Client;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Отправитель сообщений.
    /// Отправитель создается для конкретной метки сообщения и конкретной конечной точки.
    /// В случае отправки запроса с ожиданием ответа, отправитель создает получателя ответного сообщения.
    /// </summary>
    internal class Producer : IDisposable
    {
        private readonly ILog logger;
        private readonly IEndpoint endpoint;
        private readonly IRabbitConnection connection;
        private readonly object syncRoot = new object();
        private CancellationTokenSource cancellationTokenSource;
        private IPublishConfirmationTracker confirmationTracker = new DummyPublishConfirmationTracker();

        /// <summary>
        /// Initializes a new instance of the <see cref="Producer"/> class. 
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="connection">
        /// Соединение с шиной сообщений
        /// </param>
        /// <param name="label">
        /// Метка сообщения, которая будет использоваться при отправлении сообщений.
        /// </param>
        /// <param name="routeResolver">
        /// Определитель маршрутов, по которым можно отсылать и получать сообщения.
        /// </param>
        /// <param name="confirmationIsRequired">
        /// Если <c>true</c> - тогда отправитель будет ожидать подтверждения о том, что сообщение было сохранено в брокере.
        /// </param>
        public Producer(IEndpoint endpoint, IRabbitConnection connection, MessageLabel label, IRouteResolver routeResolver, bool confirmationIsRequired)
        {
            this.endpoint = endpoint;

            this.connection = connection;
            this.BrokerUrl = connection.ConnectionString;
            this.Label = label;
            this.RouteResolver = routeResolver;
            this.ConfirmationIsRequired = confirmationIsRequired;

            this.logger = LogManager.GetLogger($"{this.GetType().FullName}({this.BrokerUrl})[{this.Label}]");
        }
        
        /// <summary>
        /// Метка сообщения, которая используется для отправки сообщения.
        /// </summary>
        public MessageLabel Label { get; }
        
        /// <summary>
        /// A URL assigned to the producer to access the RabbitMQ broker
        /// </summary>
        public string BrokerUrl { get; private set; }

        /// <summary>
        /// Слушатель (получатель) ответных сообщений на запрос.
        /// </summary>
        protected Listener CallbackListener { get; private set; }

        /// <summary>
        /// Канал подключения к брокеру.
        /// </summary>
        protected RabbitChannel Channel { get; private set; }

        /// <summary>
        /// <c>true</c> - если при отправке необходимо подтверждение о том, что брокер сохранил сообщение.
        /// </summary>
        protected bool ConfirmationIsRequired { get; }

        /// <summary>
        /// Определитель маршрутов отправки и получения.
        /// </summary>
        protected IRouteResolver RouteResolver { get; }

        /// <summary>
        /// Освобождает занятые ресурсы.
        /// </summary>
        public void Dispose()
        {
            lock (this.syncRoot)
            {
                this.logger.Trace($"Disposing producer of [{this.Label}]");

                this.Stop();
                this.confirmationTracker?.Dispose();
                this.Channel?.Dispose();
            }
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
            lock (this.syncRoot)
            {
                var nativeRoute = (RabbitRoute)this.RouteResolver.Resolve(this.endpoint, message.Label);
                this.logger.Trace(m => m("Emitting message [{0}] through [{1}].", message.Label, nativeRoute));
                Action<IBasicProperties> propsVisitor = p => ApplyPublishingOptions(p, message.Headers);

                var confirmation = this.confirmationTracker.Track();
                this.Channel.Publish(nativeRoute, message, propsVisitor);
                return confirmation;
            }
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

            var timeout = Headers.Extract<TimeSpan?>(request.Headers, Headers.Timeout);
            var correlationId = (string)request.Headers[Headers.CorrelationId];

            var responseTask = this.CallbackListener.Expect(correlationId, expectedResponseType, timeout);
            
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
                this.logger.Trace($"Starting producer of [{this.Label}]");

                this.cancellationTokenSource = new CancellationTokenSource();
                var token = this.cancellationTokenSource.Token;

                this.connection.Closed += this.OnConnectionClosed;
                this.Channel = this.connection.OpenChannel(token);

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
            lock (this.syncRoot)
            {
                this.logger.Trace($"Stopping producer of [{this.Label}]");

                this.connection.Closed -= this.OnConnectionClosed;
                this.cancellationTokenSource.Cancel(true);
                this.CallbackListener?.StopConsuming();

                this.logger.Trace($"Producer of [{this.Label}] stopped successfully");
            }
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
        public void UseCallbackListener(Listener listener)
        {
            if (this.CallbackListener != null)
            {
                throw new BusConfigurationException("Callback listener for producer [{0}] is already defined.".FormatEx(this.Label));
            }

            this.CallbackListener = listener;
        }

        /// <summary>
        /// Устанавливает заголовки сообщения в свойства сообщения.
        /// </summary>
        /// <param name="props">Свойства сообщения, куда устанавливаются заголовки.</param>
        /// <param name="headers">Устанавливаемые заголовки сообщения.</param>
        private static void ApplyPublishingOptions(IBasicProperties props, IDictionary<string, object> headers)
        {
            if (headers == null)
            {
                return;
            }

            var persist = Headers.Extract<bool?>(headers, Headers.Persist);
            var ttl = Headers.Extract<TimeSpan?>(headers, Headers.Ttl);
            var correlationId = Headers.Extract<string>(headers, Headers.CorrelationId);
            var replyRoute = Headers.Extract<RabbitRoute>(headers, Headers.ReplyRoute);

            if (persist.HasValue && persist.Value)
            {
                props.DeliveryMode = 2;
            }

            if (ttl.HasValue)
            {
                props.Expiration = ttl.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            if (correlationId != null)
            {
                props.CorrelationId = correlationId;
            }

            if (replyRoute != null)
            {
                props.ReplyToAddress = new PublicationAddress("direct", replyRoute.Exchange, replyRoute.RoutingKey);
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

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            this.logger.Warn("The underlying connection has been closed, recovering the producer...");

            this.Stop();
            this.Start();
        }
    }
}
