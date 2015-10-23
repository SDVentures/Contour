using Contour.Receiving;
using Contour.Topology;
using Contour.Transport.RabbitMQ.Internal;

namespace Contour.Transport.RabbitMQ.Topology
{
    /// <summary>
    /// Построитель топологии.
    /// </summary>
    public class TopologyBuilder : ITopologyBuilder
    {
        /// <summary>
        /// Соединение с шиной сообщений, через которое настраивается топология.
        /// </summary>
        private readonly RabbitChannel rabbitChannel;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TopologyBuilder"/>.
        /// </summary>
        /// <param name="channel">
        /// Соединение с шиной сообщений, через которое настраивается топология.
        /// </param>
        public TopologyBuilder(IChannel channel)
        {
            // TODO: нужно работать с более общим типом IChannel.
            this.rabbitChannel = (RabbitChannel)channel;
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
        public void Bind(Exchange exchange, Queue queue, string routingKey = "")
        {
            this.rabbitChannel.Bind(queue, exchange, routingKey);
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
            var queue = new Queue(this.rabbitChannel.DeclareDefaultQueue());

            return new SubscriptionEndpoint(queue, new StaticRouteResolver(string.Empty, queue.Name));
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
            var queue = Queue.Named(string.Format("{0}.replies-{1}-{2}", endpoint.Address, label.IsAny ? "any" : label.Name, NameGenerator.GetRandomName(8)))
                .AutoDelete.Exclusive.Instance;

            this.rabbitChannel.Declare(queue);

            return new SubscriptionEndpoint(queue, new StaticRouteResolver(string.Empty, queue.Name));
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
            Exchange exchange = builder.Instance;

            this.rabbitChannel.Declare(exchange);

            return exchange;
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
            Queue queue = builder.Instance;

            this.rabbitChannel.Declare(queue);

            return queue;
        }
    }
}
