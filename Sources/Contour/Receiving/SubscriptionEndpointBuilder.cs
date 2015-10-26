using Contour.Sending;
using Contour.Topology;

namespace Contour.Receiving
{
    /// <summary>
    /// Построитель конечной точки подписки на ответные сообщения.
    /// </summary>
    public class SubscriptionEndpointBuilder : ISubscriptionEndpointBuilder
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SubscriptionEndpointBuilder"/>.
        /// </summary>
        /// <param name="endpoint">Конечная точка шины сообщений.</param>
        /// <param name="topology">Построитель топологии шины сообщений.</param>
        /// <param name="receiver">Конфигурация получателя ответных сообщений.</param>
        public SubscriptionEndpointBuilder(IEndpoint endpoint, ITopologyBuilder topology, IReceiverConfiguration receiver)
        {
            this.Endpoint = endpoint;
            this.Topology = topology;
            this.Receiver = receiver;
        }

        /// <summary>
        /// Конечная точка шины сообщений.
        /// </summary>
        public IEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Конфигурация получателя ответных сообщений.
        /// </summary>
        public IReceiverConfiguration Receiver { get; private set; }

        /// <summary>
        /// Построитель топологии шины сообщений.
        /// </summary>
        public ITopologyBuilder Topology { get; private set; }

        /// <summary>
        /// Создает подписку на получение ответных сообщений для указанного источника.
        /// </summary>
        /// <param name="listeningSource">Источник ответных сообщений.</param>
        /// <param name="callbackRouteResolver">Вычислитель маршрута ответного сообщения.</param>
        /// <returns>Конечная точка подписки.</returns>
        public ISubscriptionEndpoint ListenTo(IListeningSource listeningSource, IRouteResolver callbackRouteResolver)
        {
            return new SubscriptionEndpoint(listeningSource, callbackRouteResolver);
        }

        /// <summary>
        /// Создает конечную точку для ответов с настойками по умолчанию.
        /// </summary>
        /// <returns>Конечная точка подписки.</returns>
        public ISubscriptionEndpoint UseDefaultTempReplyEndpoint()
        {
            return this.Topology.BuildTempReplyEndpoint();
        }

        /// <summary>
        /// Создает конечную точку для ответов с настройками по умолчанию
        /// </summary>
        /// <param name="senderConfiguration">Настройки отправителя.</param>
        /// <returns>Конечная точка подписки на сообщения.</returns>
        public ISubscriptionEndpoint UseDefaultTempReplyEndpoint(ISenderConfiguration senderConfiguration)
        {
            return this.Topology.BuildTempReplyEndpoint(this.Endpoint, senderConfiguration.Label);
        }
    }
}
