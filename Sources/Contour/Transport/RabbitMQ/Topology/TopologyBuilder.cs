using System;
using System.Threading;
using Contour.Receiving;
using Contour.Topology;
using Contour.Transport.RabbitMQ.Internal;
using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Topology
{
    /// <summary>
    /// The topology builder.
    /// </summary>
    public class TopologyBuilder : ITopologyBuilder
    {
        private readonly RabbitChannel channel;
        private readonly IChannelProvider<IChannel> channelProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyBuilder"/> class. 
        /// </summary>
        /// <param name="channel">
        /// Соединение с шиной сообщений, через которое настраивается топология.
        /// </param>
        [Obsolete("Use a connection based constructor")]
        public TopologyBuilder(IChannel channel)
        {
            this.channel = (RabbitChannel)channel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyBuilder"/> class.
        /// </summary>
        /// <param name="channelProvider">
        /// The provider.
        /// </param>
        public TopologyBuilder(IChannelProvider<IChannel> channelProvider)
        {
            this.channelProvider = channelProvider;
        }

        /// <summary>
        /// Привязывает точку обмена (маршрутизации) с очередью.
        /// </summary>
        /// <param name="exchange">
        /// Точка обмена (маршрутизации) сообщений, на которую поступают сообщения. Точка обмена действует на основе ключа маршрутизации <paramref name="routingKey"/>.
        /// </param>
        /// <param name="queue">
        /// Очередь, в которую будут поступать сообщения из маршрутизатора.
        /// </param>
        /// <param name="routingKey">
        /// Ключ маршрутизации, используется для определения очереди, в которую должно быть отправлено сообщение.
        /// </param>
        public void Bind(Exchange exchange, Queue queue, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            using (var channel = (RabbitChannel)this.channelProvider.OpenChannel(CancellationToken.None))
            {
                channel.Bind(queue, exchange, routingKey, arguments);
            }
        }

        /// <summary>
        /// Создает временную конечную точку подписки на сообщения.
        /// Обычно используется для организации варианта коммуникации: запрос-ответ.
        /// </summary>
        /// <returns>
        /// Конечная точка подписки на сообщения <see cref="ISubscriptionEndpoint"/>.
        /// </returns>
        public ISubscriptionEndpoint BuildTempReplyEndpoint()
        {
            using (var channel = (RabbitChannel)this.channelProvider.OpenChannel(CancellationToken.None))
            {
                var queue = new Queue(channel.DeclareDefaultQueue());
                return new SubscriptionEndpoint(queue, new StaticRouteResolver(string.Empty, queue.Name));
            }
        }

        /// <summary>
        /// Создает временную конечную точку для получения сообщений.
        /// </summary>
        /// <param name="endpoint">Конечная точка шины сообщений для который создается подписка.</param>
        /// <param name="label">Метка сообщений, на которые ожидается получение ответа.</param>
        /// <returns>
        /// Конечная точка подписки для получения сообщений.
        /// </returns>
        public ISubscriptionEndpoint BuildTempReplyEndpoint(IEndpoint endpoint, MessageLabel label)
        {
            using (var channel = (RabbitChannel)this.channelProvider.OpenChannel(CancellationToken.None))
            {
                var queue =
                    Queue.Named(
                        $"{endpoint.Address}.replies-{(label.IsAny ? "any" : label.Name)}-{NameGenerator.GetRandomName(8)}")
                        .AutoDelete.Exclusive.Instance;

                channel.Declare(queue);

                return new SubscriptionEndpoint(queue, new StaticRouteResolver(string.Empty, queue.Name));
            }
        }

        /// <summary>
        /// Создает точку обмена (маршрутизации), на которую поступают сообщения в брокере.
        /// </summary>
        /// <param name="builder">
        /// Построитель точки обмена (маршрутизации).
        /// </param>
        /// <returns>
        /// Точка обмена (маршрутизации) <see cref="Exchange"/>.
        /// </returns>
        public Exchange Declare(ExchangeBuilder builder)
        {
            using (var channel = (RabbitChannel)this.channelProvider.OpenChannel(CancellationToken.None))
            {
                var exchange = builder.Instance;
                channel.Declare(exchange);
                return exchange;
            }
        }

        /// <summary>
        /// Создает очередь в брокере для всех сообщений готовых к обработке.
        /// </summary>
        /// <param name="builder">
        /// Построитель очереди.
        /// </param>
        /// <returns>
        /// Очередь сообщений <see cref="Queue"/> для обработки.
        /// </returns>
        public Queue Declare(QueueBuilder builder)
        {
            using (var channel = (RabbitChannel)this.channelProvider.OpenChannel(CancellationToken.None))
            {
                var queue = builder.Instance;
                channel.Declare(queue);
                return queue;
            }
        }

        /// <summary>
        /// Disposes of the topology builder
        /// </summary>
        public void Dispose()
        {
            this.channel?.Dispose();
        }
    }
}
