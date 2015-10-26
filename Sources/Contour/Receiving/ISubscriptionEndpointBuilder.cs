using Contour.Sending;
using Contour.Topology;

namespace Contour.Receiving
{
    /// <summary>
    /// Построитель конечной точки для получения сообщения.
    /// </summary>
    public interface ISubscriptionEndpointBuilder
    {
        /// <summary>
        /// Конечная точка шины сообщений.
        /// </summary>
        IEndpoint Endpoint { get; }

        /// <summary>
        /// Конфигурация получателя.
        /// </summary>
        IReceiverConfiguration Receiver { get; }

        /// <summary>
        /// Построитель топологии.
        /// </summary>
        ITopologyBuilder Topology { get; }

        /// <summary>
        /// Создает конечную точку подписки на указанный источник
        /// </summary>
        /// <param name="listeningSource">Источник подписки.</param>
        /// <param name="callbackRouteResolver">Вычислитель маршрута ответного сообщения.</param>
        /// <returns>Конечная точка подписки.</returns>
        ISubscriptionEndpoint ListenTo(IListeningSource listeningSource, IRouteResolver callbackRouteResolver = null);

        /// <summary>
        /// Устанавливает конечную точку для ответов с настройками по умолчанию.
        /// </summary>
        /// <returns>Конечная точка подписки на сообщения.</returns>
        ISubscriptionEndpoint UseDefaultTempReplyEndpoint();

        /// <summary>
        /// Использовать конечную точку для ответов с настройками по умолчанию
        /// </summary>
        /// <param name="senderConfiguratoration">Настройки отправителя.</param>
        /// <returns>Конечная точка подписки на сообщения.</returns>
        ISubscriptionEndpoint UseDefaultTempReplyEndpoint(ISenderConfiguration senderConfiguratoration);
    }
}
