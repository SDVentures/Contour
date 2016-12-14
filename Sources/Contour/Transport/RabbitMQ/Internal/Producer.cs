namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using Common.Logging;

    using Contour.Configuration;
    using Contour.Sending;

    using global::RabbitMQ.Client;

    /// <summary>
    /// Отправитель сообщений.
    /// Отправитель создается для конкретной метки сообщения и конкретной конечной точки.
    /// В случае отправки запроса с ожиданием ответа, отправитель создает получателя ответного сообщения.
    /// </summary>
    internal class Producer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Отслеживает подтверждение ответов.
        /// </summary>
        private readonly IPublishConfirmationTracker confirmationTracker = new NullPublishConfirmationTracker();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Producer"/>.
        /// </summary>
        /// <param name="bus">
        /// Конечная точка, для которой создается отправитель.
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
        public Producer(IRabbitBus bus, MessageLabel label, IRouteResolver routeResolver, bool confirmationIsRequired)
        {
            var provider = (IChannelProvider) bus;
            this.Channel = (RabbitChannel) provider.OpenChannel();
            this.Label = label;
            this.RouteResolver = routeResolver;
            this.ConfirmationIsRequired = confirmationIsRequired;

            if (this.ConfirmationIsRequired)
            {
                this.confirmationTracker = new DefaultPublishConfirmationTracker(this.Channel);
                this.Channel.EnablePublishConfirmation();
                this.Channel.OnConfirmation(this.confirmationTracker.HandleConfirmation);
            }

            this.Failed += _ => ((IBusAdvanced)bus).Panic();
        }

        /// <summary>
        /// Событие о провале выполнения операции.
        /// </summary>
        public event Action<Producer> Failed = p => { };

        /// <summary>
        /// Метка сообщения, которая используется для отправки сообщения.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        /// <c>true</c>, если в отправителе произошел отказ.
        /// </summary>
        public bool HasFailed
        {
            get
            {
                return !this.Channel.IsOpen;
            }
        }

        /// <summary>
        /// Слушатель (получатель) ответных сообщений на запрос.
        /// </summary>
        protected RabbitListener CallbackListener { get; private set; }

        /// <summary>
        /// Канал подключения к брокеру.
        /// </summary>
        protected RabbitChannel Channel { get; private set; }

        /// <summary>
        /// <c>true</c> - если при отправке необходимо подтверждение о том, что брокер сохранил сообщение.
        /// </summary>
        protected bool ConfirmationIsRequired { get; private set; }

        /// <summary>
        /// Определитель маршрутов отправки и получения.
        /// </summary>
        protected IRouteResolver RouteResolver { get; private set; }

        /// <summary>
        /// Освобождает занятые ресурсы.
        /// </summary>
        public void Dispose()
        {
            Logger.Trace(m => m("Disposing producer of [{0}].", this.Label));

            if (this.confirmationTracker != null)
            {
                this.confirmationTracker.Dispose();
            }

            if (this.Channel != null)
            {
                this.Channel.Dispose();
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
            var nativeRoute = (RabbitRoute)this.RouteResolver.Resolve(this.Channel.Bus.Endpoint, message.Label);

            Logger.Trace(m => m("Emitting message [{0}] through [{1}].", message.Label, nativeRoute));

            Action<IBasicProperties> propsVisitor = p => ApplyPublishingOptions(p, message.Headers);

            // getting next seqno and publishing should go together
            lock (this.confirmationTracker)
            {
                Task confirmation = this.confirmationTracker.Track();
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

            Task<IMessage> responseTask = this.CallbackListener.Expect(correlationId, expectedResponseType, timeout);

            // TODO: join with responseTask (AttachToParent has proven to be too slow)
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
            Logger.Trace(m => m("Starting producer of [{0}].", this.Label));
            if (this.CallbackListener != null)
            {
                this.CallbackListener.Start();
            }
        }

        /// <summary>
        /// Останавливает отправитель, после этого ответы на запросы не будут обрабатываться.
        /// </summary>
        public void Stop()
        {
            Logger.Trace(m => m("Stopping producer of [{0}].", this.Label));
            if (this.CallbackListener != null)
            {
                this.CallbackListener.Stop();
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
        public void UseCallbackListener(RabbitListener listener)
        {
            if (this.CallbackListener != null)
            {
                throw new BusConfigurationException("Callback listener for producer [{0}] is already defined.".FormatEx(this.Label));
            }

            this.CallbackListener = listener;
            this.CallbackListener.Failed += l =>
                {
                    this.CallbackListener = null;
                    this.Failed(this);
                };
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
                throw new BusConfigurationException(string.Format("No reply endpoint is defined for publisher of [{0}].", this.Label));
            }

            if (this.CallbackListener.Endpoint.CallbackRouteResolver == null)
            {
                throw new BusConfigurationException(string.Format("No callback route resolver is defined for listener on [{0}].", this.CallbackListener.Endpoint.ListeningSource));
            }

            return this.CallbackListener.Endpoint.CallbackRouteResolver.Resolve(this.Channel.Bus.Endpoint, MessageLabel.Any);
        }
    }
}
