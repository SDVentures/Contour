using System;

using Contour.Receiving;

namespace Contour.Sending
{
    /// <summary>
    /// Конфигуратор отправителя.
    /// </summary>
    public interface ISenderConfigurator
    {
        /// <summary>
        /// Создает конфигуратор на основе построителя вычислителя маршрутов.
        /// </summary>
        /// <param name="routeResolverBuilder">
        /// Построитель вычислителя маршрутов.
        /// </param>
        /// <returns>
        /// Конфигуратор отправителя.
        /// </returns>
        ISenderConfigurator ConfiguredWith(Func<IRouteResolverBuilder, IRouteResolver> routeResolverBuilder);

        /// <summary>
        /// Сообщения должны сохраняться на диск для надежной доставки.
        /// </summary>
        /// <returns>
        /// Конфигуратор отправителя.
        /// </returns>
        ISenderConfigurator Persistently();

        /// <summary>
        /// Устанавливает псевдоним метки отправляемого сообщения.
        /// </summary>
        /// <param name="alias">Псевдоним метки отправляемого сообщения.</param>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithAlias(string alias);

        /// <summary>
        /// Устанавливает построитель конечной точки получения ответа.
        /// </summary>
        /// <param name="callbackEndpointBuilder">Построитель конечной точки для получения ответных сообщений.</param>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithCallbackEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> callbackEndpointBuilder);

        /// <summary>
        /// Устанавливает, что требуется подтверждение получения сообщения брокером.
        /// </summary>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithConfirmation();

        /// <summary>
        /// Для получения ответного сообщения должна использоваться точка подписки, формируемая по умолчанию используемым провайдером.
        /// </summary>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithDefaultCallbackEndpoint();

        /// <summary>
        /// Устанавливает максимальное время ожидания ответа на запрос.
        /// </summary>
        /// <param name="timeout">Время ожидания ответа на запрос.</param>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithRequestTimeout(TimeSpan? timeout);

        /// <summary>
        /// Устанавливает TTL (время жизни) для отправляемых сообщений.
        /// </summary>
        /// <param name="ttl">Желаемое время жизни сообщений.</param>
        /// <returns>Конфигуратор отправителя.</returns>
        ISenderConfigurator WithTtl(TimeSpan ttl);

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор отправителя с установленным хранилище заголовков входящего сообщения.</returns>
        ISenderConfigurator WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage);
    }
}
