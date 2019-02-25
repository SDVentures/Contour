using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    internal sealed class Producer : IProducer
    {
        private readonly ILog logger;
        private readonly IEndpoint endpoint;
        private readonly IRabbitConnection connection;
        private readonly ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
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
        private RabbitChannel Channel { get; set; }

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
            if (this.slimLock.TryEnterReadLock(TimeSpan.Zero))
            {
                try
                {
                    var nativeRoute = (RabbitRoute)this.RouteResolver.Resolve(this.endpoint, message.Label);
                    this.logger.Trace(m => m("Emitting message [{0}] through [{1}].", message.Label, nativeRoute));
                    Func<IBasicProperties, IDictionary<string, object>> propsVisitor = p => ExtractProperties(ref p, message.Headers);

                    var confirmation = this.confirmationTracker.Track();
                    this.Channel.Publish(nativeRoute, message, propsVisitor);
                    return confirmation;
                }
                finally
                {
                    try
                    {
                        this.slimLock.ExitReadLock();
                    }
                    catch (Exception)
                    {
                        // Suppress all errors here
                    }
                }
            }

            throw new Exception($"Unable to publish via producer [{this.GetHashCode()}] because it is not yet started or is recovering");
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
            try
            {
                this.logger.Trace($"Starting producer [{this.GetHashCode()}] of [{this.Label}]");
                this.slimLock.EnterWriteLock();
                
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
            finally
            {
                try
                {
                    this.slimLock.ExitWriteLock();
                }
                catch (Exception)
                {
                    // Suppress all errors
                }
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

        /// <summary>
        /// Устанавливает заголовки сообщения в свойства сообщения.
        /// </summary>
        /// <param name="props">
        /// Свойства сообщения, куда устанавливаются заголовки.
        /// </param>
        /// <param name="sourceHeaders">
        /// The source Headers.
        /// </param>
        /// <returns>
        /// The <see cref="IDictionary{K,V}"/>.
        /// </returns>
        private static IDictionary<string, object> ExtractProperties(ref IBasicProperties props, IDictionary<string, object> sourceHeaders)
        {
            var headers = new Dictionary<string, object>(sourceHeaders);

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

            return headers;
        }


        private void InternalStop(OperationStopReason reason)
        {
            try
            {
                this.logger.Trace($"Stopping producer of [{this.Label}]");
                this.slimLock.EnterWriteLock();
                
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
            finally
            {
                try
                {
                    this.slimLock.ExitWriteLock();
                }
                catch (Exception)
                {
                    // Suppress all errors
                }
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
