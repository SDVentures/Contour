namespace Contour.Transport.RabbitMq
{
    /// <summary>
    ///   Описание маршрута для доставки сообщений через RabbitMQ.
    /// </summary>
    public class RabbitRoute : IRoute
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitRoute"/>. 
        /// Создает описание машрута для доставки сообщений через RabbitMQ.
        /// </summary>
        /// <param name="exchange">
        /// Exchange.
        /// </param>
        /// <param name="routingKey">
        /// Routing key.
        /// </param>
        public RabbitRoute(string exchange, string routingKey = "")
        {
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
        }
        /// <summary>
        ///   Имя exchange.
        /// </summary>
        public string Exchange { get; private set; }

        /// <summary>
        ///   Routing key.
        /// </summary>
        public string RoutingKey { get; private set; }
        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Exchange: {0}, RoutingKey: {1}", this.Exchange, this.RoutingKey);
        }
    }
}
