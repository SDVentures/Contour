using System;
using Contour.Helpers;
using Contour.Receiving.Consumers;
using Contour.Validation;

namespace Contour.Receiving
{
    /// <summary>
    /// Конфигуратор получателя сообщения.
    /// </summary>
    public interface IReceiverConfigurator
    {
        /// <summary>
        /// Возвращает типизированный конфигуратор получателя.
        /// </summary>
        /// <typeparam name="T">Тип конфигуратора получателя.</typeparam>
        /// <returns>Конфигуратор получателя.</returns>
        IReceiverConfigurator<T> As<T>() where T : class;

        /// <summary>
        /// Регистрирует стратегию обработки сообщений, получение которых завершилось провалом.
        /// </summary>
        /// <param name="failedDeliveryStrategy">Стратегия обработки сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений.</returns>
        IReceiverConfigurator OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy);

        /// <summary>Регистрирует обработчик сообщений, получение которых завершилось провалом.</summary>
        /// <param name="failedDeliveryHandler">Обработчик сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений</returns>
        IReceiverConfigurator OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler);

        /// <summary>Регистрирует фабрику обработчиков входящего сообщения.</summary>
        /// <param name="consumerFactoryFunc">Фабрика обработчиков входящих сообщений.</param>
        /// <typeparam name="T">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        IReceiverConfigurator<T> ReactWith<T>(Func<IConsumerOf<T>> consumerFactoryFunc) where T : class;

        /// <summary>Регистрирует обработчик входящего сообщения.</summary>
        /// <param name="consumer">Обработчик входящего сообщения.</param>
        /// <typeparam name="T">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        IReceiverConfigurator<T> ReactWith<T>(IConsumer<T> consumer) where T : class;

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки сообщения.
        /// </summary>
        /// <returns>
        /// Конфигуратор получателя с явным подтверждением успешной обработки сообщения.
        /// </returns>
        IReceiverConfigurator RequiresAccept();

        /// <summary>
        /// Регистрирует механизм проверки входящего сообщения.
        /// </summary>
        /// <param name="validator">Механизм проверки входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным механизмом проверки входящих сообщений.</returns>
        IReceiverConfigurator WhenVerifiedBy(IMessageValidator validator);

        /// <summary>
        /// Устанавливает псевдоним входящих сообщений.
        /// </summary>
        /// <param name="alias">Псевдоним входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным псевдонимом входящих сообщений.</returns>
        IReceiverConfigurator WithAlias(string alias);

        /// <summary>
        /// Регистрирует построитель порта, по которому проходит получение входящих сообщений.
        /// </summary>
        /// <param name="endpointBuilder">Построитель порта входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным построителем порта входящих сообщений.</returns>
        IReceiverConfigurator WithEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder);

        /// <summary>
        /// Устанавливает количество одновременных обработчиков входящих сообщений.
        /// </summary>
        /// <param name="parallelismLevel">Количество одновременных обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным количеством одновременных обработчиков входящих сообщений.</returns>
        IReceiverConfigurator WithParallelismLevel(uint parallelismLevel);
        
        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным хранилищем заголовков.</returns>
        IReceiverConfigurator WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage);

        IReceiverConfigurator WithQueueLimit(int queueLimit);
        IReceiverConfigurator WithQueueMaxLengthBytes(int maxLengthBytes);

        /// <summary>
        /// Устанавливает ограничение на подключение только к отправителям, поддерживающим задержку доставки сообщений.
        /// </summary>
        /// <returns>Конфигуратор получателя.</returns>
        IReceiverConfigurator Delayed();

        IReceiverConfigurator Direct(string id);
    }

    /// <summary>
    /// Конфигуратор получателя сообщения.
    /// </summary>
    /// <typeparam name="T">Тип получаемого сообщения.</typeparam>
    public interface IReceiverConfigurator<T> : IReceiverConfigurator
        where T : class
    {
        /// <summary>
        /// Регистрирует стратегию обработки сообщений, получение которых завершилось провалом.
        /// </summary>
        /// <param name="failedDeliveryStrategy">Стратегия обработки сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений.</returns>
        new IReceiverConfigurator<T> OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy);

        /// <summary>Регистрирует обработчик сообщений, получение которых завершилось провалом.</summary>
        /// <param name="failedDeliveryHandler">Обработчик сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений</returns>
        new IReceiverConfigurator<T> OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler);

        /// <summary>Регистрирует фабрику обработчиков входящего сообщения.</summary>
        /// <param name="consumerFactoryFunc">Фабрика обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        IReceiverConfigurator<T> ReactWith(Func<IConsumerOf<T>> consumerFactoryFunc);

        /// <summary>Регистрирует обработчик входящего сообщения.</summary>
        /// <param name="consumer">Обработчик входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        IReceiverConfigurator<T> ReactWith(IConsumerOf<T> consumer);

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки сообщения.
        /// </summary>
        /// <returns>
        /// Конфигуратор получателя с явным подтверждением успешной обработки сообщения.
        /// </returns>
        new IReceiverConfigurator<T> RequiresAccept();

        /// <summary>
        /// Регистрирует механизм проверки входящего сообщения.
        /// </summary>
        /// <param name="validator">Механизм проверки входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным механизмом проверки входящих сообщений.</returns>
        IReceiverConfigurator<T> WhenVerifiedBy(IMessageValidatorOf<T> validator);

        /// <summary>
        /// Устанавливает псевдоним входящих сообщений.
        /// </summary>
        /// <param name="alias">Псевдоним входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным псевдонимом входящих сообщений.</returns>
        new IReceiverConfigurator<T> WithAlias(string alias);

        /// <summary>
        /// Регистрирует построитель порта, по которому проходит получение входящих сообщений.
        /// </summary>
        /// <param name="endpointBuilder">Построитель порта входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным построителем порта входящих сообщений.</returns>
        new IReceiverConfigurator<T> WithEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder);

        /// <summary>
        /// Устанавливает количество одновременных обработчиков входящих сообщений.
        /// </summary>
        /// <param name="parallelismLevel">Количество одновременных обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным количеством одновременных обработчиков входящих сообщений.</returns>
        new IReceiverConfigurator<T> WithParallelismLevel(uint parallelismLevel);

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным хранилищем заголовков.</returns>
        new IReceiverConfigurator WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage);

        new IReceiverConfigurator WithQueueLimit(int queueLimit);
        new IReceiverConfigurator WithQueueMaxLengthBytes(int maxLengthBytes);
    }
}
