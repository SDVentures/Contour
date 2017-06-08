using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Contour.Configuration;
using Contour.Receiving;
using Contour.Serialization;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// Доставленное сообщение через брокер <c>RabbitMQ</c>.
    /// </summary>
    internal class RabbitDelivery : IDelivery
    {
        /// <summary>
        /// Заголовки сообщения.
        /// </summary>
        private readonly Lazy<IDictionary<string, object>> headers;

        /// <summary>
        /// Верно, если сообщение требует подтверждения обработки.
        /// </summary>
        private readonly bool requiresAccept;

        private readonly IPayloadConverter payloadConverter;

        private readonly IBusContext busContext;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private volatile bool isAccepted;

        public RabbitDelivery(IBusContext busContext, IRabbitChannel channel, BasicDeliverEventArgs args, bool requiresAccept, IPayloadConverter payloadConverter)
        {
            this.busContext = busContext;

            this.Channel = channel;
            this.Label = this.busContext.MessageLabelHandler.Resolve(args);
            this.Args = args;
            this.requiresAccept = requiresAccept;
            this.payloadConverter = payloadConverter;

            this.headers = new Lazy<IDictionary<string, object>>(
                () => this.ExtractHeadersFrom(args));
        }

        /// <summary>
        /// Параметры поставки сообщения.
        /// </summary>
        public BasicDeliverEventArgs Args { get; private set; }

        /// <summary>
        /// Канал доставки сообщения.
        /// </summary>
        public IRabbitChannel Channel { get; private set; }
        
        /// <summary>
        /// Формат содержимого сообщения.
        /// </summary>
        public string ContentType => this.Args.BasicProperties.ContentType;

        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        public string CorrelationId => this.Args.BasicProperties.CorrelationId;

        /// <summary>
        /// Заголовки сообщения.
        /// </summary>
        public IDictionary<string, object> Headers => this.headers.Value;

        /// <summary>
        /// Маршрут входящего сообщения.
        /// </summary>
        public RabbitRoute IncomingRoute => new RabbitRoute(this.Args.Exchange, this.Args.RoutingKey);

        /// <summary>
        /// Верно, если сообщение запрос.
        /// </summary>
        public bool IsRequest => this.CorrelationId != null && this.ReplyRoute != null;

        /// <summary>
        /// Верно, если сообщение ответ.
        /// </summary>
        public bool IsResponse => this.CorrelationId != null && this.ReplyRoute == null;

        /// <summary>
        /// Метка полученного сообщения.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        /// Верно, если можно ответить на полученное сообщение.
        /// </summary>
        public bool CanReply => this.ReplyRoute != null && !string.IsNullOrWhiteSpace(this.CorrelationId);

        /// <summary>
        /// Канал доставки сообщения.
        /// </summary>
        IChannel IDelivery.Channel => this.Channel;

        /// <summary>
        /// Маршрут ответа на доставленное сообщение.
        /// </summary>
        IRoute IDelivery.ReplyRoute => this.ReplyRoute;

        /// <summary>
        /// Маршрут ответа на доставленное сообщение.
        /// </summary>
        private RabbitRoute ReplyRoute
        {
            get
            {
                if (this.Args.BasicProperties.ReplyTo == null)
                {
                    return null;
                }

                PublicationAddress replyAddress = this.Args.BasicProperties.ReplyToAddress;
                return new RabbitRoute(replyAddress.ExchangeName, replyAddress.RoutingKey);
            }
        }

        /// <summary>
        /// Подтверждает доставку сообщения.
        /// </summary>
        public void Accept()
        {
            if (!this.requiresAccept || this.isAccepted)
            {
                return;
            }

            this.Channel.Accept(this);
            this.isAccepted = true;
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <typeparam name="T">Тип получаемого сообщения.</typeparam>
        /// <returns>Задача пересылки сообщения.</returns>
        public IConsumingContext<T> BuildConsumingContext<T>(MessageLabel label = null) where T : class
        {
            Message<T> message = this.UnpackAs<T>();

            if (label != null && !label.IsAny && !label.IsEmpty)
            {
                message = (Message<T>)message.WithLabel(label);
            }

            return new DefaultConsumingContext<T>(this.busContext, message, this);
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <returns>Задачи пересылки сообщения.</returns>
        public Task Forward(MessageLabel label, object payload)
        {
            var headers = new Dictionary<string, object>(this.Headers);
            headers[Contour.Headers.CorrelationId] = this.CorrelationId;
            headers[Contour.Headers.ReplyRoute] = this.ReplyRoute;
            Contour.Headers.ApplyBreadcrumbs(headers, this.busContext.Endpoint.Address);
            Contour.Headers.ApplyOriginalMessageId(headers);

            return this.busContext.Emit(label, payload, headers);
        }

        /// <summary>
        /// Помечает сообщение как необработанное.
        /// </summary>
        /// <param name="requeue">
        /// Сообщение требуется вернуть во входящую очередь для повторной обработки.
        /// </param>
        public void Reject(bool requeue)
        {
            if (!this.requiresAccept || this.isAccepted)
            {
                return;
            }

            this.Channel.Reject(this, requeue);
            this.isAccepted = true;
        }

        /// <summary>
        /// Отсылает ответное сообщение.
        /// </summary>
        /// <param name="message">Ответное сообщение.</param>
        public void ReplyWith(IMessage message)
        {
            if (!string.IsNullOrEmpty(this.ContentType))
            {
                message.Headers[Contour.Headers.ContentType] = this.ContentType;
            }

            if (!this.CanReply)
            {
                throw new BusConfigurationException("No ReplyToAddress or CorrelationId were found in delivery. Make sure that you have callback endpoint set.");
            }

            message.Headers.Add(Contour.Headers.CorrelationId, this.CorrelationId);

            this.Channel.Publish(
                new RabbitRoute(this.ReplyRoute.Exchange, this.ReplyRoute.RoutingKey), 
                message, 
                this.payloadConverter);
        }

        /// <summary>
        /// Конвертирует полученную информацию в сообщение указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип сообщения.</typeparam>
        /// <returns>Сообщение указанного типа.</returns>
        public Message<T> UnpackAs<T>() where T : class
        {
            IMessage message = this.Channel.UnpackAs(typeof(T), this, this.payloadConverter);
            return new Message<T>(message.Label, message.Headers, (T)message.Payload);
        }

        /// <summary>
        /// Конвертирует полученную информацию в сообщение указанного типа.
        /// </summary>
        /// <param name="type">Тип сообщения.</param>
        /// <returns>Сообщение указанного типа.</returns>
        public IMessage UnpackAs(Type type)
        {
            return this.Channel.UnpackAs(type, this, this.payloadConverter);
        }

        /// <summary>
        /// Вычленяет заголовки из параметров сообщения.
        /// </summary>
        /// <param name="args">Параметры отправки сообщения.</param>
        /// <returns>Заголовки сообщения.</returns>
        private IDictionary<string, object> ExtractHeadersFrom(BasicDeliverEventArgs args)
        {
            var h = new Dictionary<string, object>(args.BasicProperties.Headers);

            if (this.CorrelationId != null)
            {
                h[Contour.Headers.CorrelationId] = this.CorrelationId;
            }

            if (this.ReplyRoute != null)
            {
                h[Contour.Headers.ReplyRoute] = this.ReplyRoute;
            }

            return h;
        }
    }
}
