using System.Collections.Generic;
using Contour.Configurator;

namespace Contour.Configuration
{
    using System;

    using Contour.Filters;
    using Contour.Receiving;
    using Contour.Sending;
    using Contour.Serialization;
    using Contour.Validation;

    /// <summary>
    ///   Интерфейс для конфигурирования экземпляра шины событий
    /// </summary>
    public interface IBusConfigurator
    {
        /// <summary>
        /// Устанавливает обработчик жизненного цикла конечной точки.
        /// </summary>
        /// <param name="lifecycleHandler">
        /// Обработчик жизненного цикла.
        /// </param>
        void HandleLifecycleWith(IBusLifecycleHandler lifecycleHandler);

        /// <summary>
        /// Регистрирует обработчика сообщения указанного типа.
        /// </summary>
        /// <remarks>
        /// Возможен только один обработчик сообщения указанного типа.
        /// </remarks>
        /// <typeparam name="T">
        /// .NET тип получаемого сообщения.
        /// </typeparam>
        /// <param name="label">
        /// Тип сообщения (строковая костанта).
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Генерирует исключение в случае повторной регистрации получателя на
        ///   указанный тип сообщения.
        /// </exception>
        IReceiverConfigurator<T> On<T>(string label) where T : class;

        /// <summary>
        /// Регистрирует обработчика сообщения указанного типа.
        /// </summary>
        /// <remarks>
        /// Возможен только один обработчик сообщения указанного типа.
        /// </remarks>
        /// <typeparam name="T">
        /// .NET тип получаемого сообщения.
        /// </typeparam>
        /// <param name="label">
        /// Тип сообщения.
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Генерирует исключение в случае повторной регистрации получателя на
        ///   указанный тип сообщения.
        /// </exception>
        IReceiverConfigurator<T> On<T>(MessageLabel label) where T : class;

        /// <summary>
        ///   Регистрирует обработчика сообщения, с меткой указанной в аттрибуте класса.
        /// </summary>
        /// <remarks>
        ///   Возможен только один обработчик сообщения указанного типа.
        /// </remarks>
        /// <typeparam name="T">.NET тип получаемого сообщения.</typeparam>
        /// <returns>Конфигурация экземпляра шины сообщений.</returns>
        /// <exception cref="InvalidOperationException">
        ///   Генерирует исключение в случае повторной регистрации получателя на
        ///   указанный тип сообщения.
        /// </exception>
        IReceiverConfigurator<T> On<T>() where T : class;

        /// <summary>
        /// Регистрирует обработчика сообщения указанного типа.
        /// </summary>
        /// <remarks>
        /// Возможен только один обработчик сообщения указанного типа.
        /// </remarks>
        /// <param name="label">
        /// Тип сообщения.
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Генерирует исключение в случае повторной регистрации получателя на
        ///   указанный тип сообщения.
        /// </exception>
        IReceiverConfigurator On(string label);

        /// <summary>
        /// Регистрирует обработчика сообщения указанного типа.
        /// </summary>
        /// <remarks>
        /// Возможен только один обработчик сообщения указанного типа.
        /// </remarks>
        /// <param name="label">
        /// Тип сообщения.
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Генерирует исключение в случае повторной регистрации получателя на
        ///   указанный тип сообщения.
        /// </exception>
        IReceiverConfigurator On(MessageLabel label);

        /// <summary>
        /// Устанавливает обработчик обработанных с ошибкой сообщений.
        /// </summary>
        /// <param name="failedDeliveryStrategy">
        /// Стратегия обработки некорректно обработанных сообщений.
        /// </param>
        void OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy);

        /// <summary>
        /// Устанавливает обработчик обработанных с ошибкой сообщений.
        /// </summary>
        /// <param name="failedDeliveryHandler">
        /// Обработчик некорректно обработанных сообщений.
        /// </param>
        void OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler);

        /// <summary>
        /// Устанавливает обработчик недоставленных (с отсутствующей подпиской) сообщений.
        /// </summary>
        /// <param name="unhandledDeliveryStrategy">
        /// Стратегия обработки недоставленных сообщений.
        /// </param>
        void OnUnhandled(IUnhandledDeliveryStrategy unhandledDeliveryStrategy);

        /// <summary>
        /// Устанавливает обработчик недоставленных (с отсутствующей подпиской) сообщений.
        /// </summary>
        /// <param name="unhandledDeliveryHandler">
        /// Обработчик недоставленного сообщения.
        /// </param>
        void OnUnhandled(Action<IFaultedConsumingContext> unhandledDeliveryHandler);

        /// <summary>
        /// The register filter.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        void RegisterFilter(IMessageExchangeFilter filter);

        /// <summary>
        /// Регистрирует конкретный валидатор тела сообщения.
        /// </summary>
        /// <param name="validator">
        /// Валидатор.
        /// </param>
        void RegisterValidator(IMessageValidator validator);

        /// <summary>
        /// Регистрирует группу валидаторов тела сообщения.
        /// </summary>
        /// <param name="validatorGroup">
        /// Группа валидаторов.
        /// </param>
        void RegisterValidators(MessageValidatorGroup validatorGroup);

        /// <summary>
        /// Регистрирует сообщение указанного типа.
        /// </summary>
        /// <remarks>
        /// Необходимо зарегистрировать все исходящие сообщения.
        /// </remarks>
        /// <param name="label">
        /// Тип сообщения (строковая константа).
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        ISenderConfigurator Route(string label);

        /// <summary>
        /// Регистрирует сообщение указанного типа.
        /// </summary>
        /// <remarks>
        /// Необходимо зарегистрировать все исходящие сообщения.
        /// </remarks>
        /// <param name="label">
        /// Тип сообщения.
        /// </param>
        /// <returns>
        /// Конфигурация экземпляра шины сообщений.
        /// </returns>
        ISenderConfigurator Route(MessageLabel label);

        /// <summary>
        ///   Регистрирует сообщение, получая метку из аттрибута указанного типа.
        /// </summary>
        /// <remarks>
        ///   Необходимо зарегистрировать все исходящие сообщения.
        /// </remarks>
        /// <returns>Конфигурация экземпляра шины сообщений.</returns>
        ISenderConfigurator Route<T>() where T : class;

        /// <summary>
        /// Устанавливает строку соединения с брокером.
        /// </summary>
        /// <param name="connectionString">
        /// Строка подключения к брокеру.
        /// </param>
        void SetConnectionString(string connectionString);

        /// <summary>
        /// Sets connection reuse.
        /// </summary>
        /// <param name="reuse">
        /// Connection reuse flag.
        /// </param>
        void ReuseConnection(bool reuse = true);

        /// <summary>
        /// Устанавливает адрес конечной точки приложения.
        /// </summary>
        /// <param name="address">
        /// Адрес конечной точки приложения.
        /// </param>
        void SetEndpoint(string address);

        /// <summary>
        /// Устанавливает уровень параллелизма (количество потоков обработки сообщений) по умолчанию.
        /// </summary>
        /// <param name="parallelismLevel">
        /// Уровень параллелизма.
        /// </param>
        void UseParallelismLevel(uint parallelismLevel);

        /// <summary>
        /// The fault queue message TTL.
        /// </summary>
        /// <param name="messageTtl">the fault queue message TTL.</param>
        void UseFaultQueueTtl(TimeSpan messageTtl);

        /// <summary>
        /// The fault message queue length limit.
        /// </summary>
        /// <param name="queueLimit">The fault message queue length limit.</param>
        void UseFaultQueueLimit(int queueLimit);

        /// <summary>
        /// The message queue length limit.
        /// </summary>
        /// <param name="queueLimit">The message queue length limit.</param>
        void UseQueueLimit(int queueLimit);

        /// <summary>
        /// The message queue length limit in bytes.
        /// </summary>
        /// <param name="bytes">The message queue limit in bytes.</param>
        void UseQueueMaxLengthBytes(int bytes);

        /// <summary>
        /// Устанаваливает конвертер тела сообщений.
        /// </summary>
        /// <param name="converter">
        /// </param>
        void UsePayloadConverter(IPayloadConverter converter);

        /// <summary>
        /// Устанавливает таймаут для запросов по умолчанию.
        /// </summary>
        /// <param name="timeout">
        /// Таймаут.
        /// </param>
        void UseRequestTimeout(TimeSpan? timeout);

        /// <summary>
        /// Устанавливает конфигуратор
        /// </summary>
        /// <param name="routeResolverBuilder">
        /// </param>
        void UseRouteResolverBuilder(Func<IRouteResolverBuilder, IRouteResolver> routeResolverBuilder);

        /// <summary>
        /// Устанавливает обработчик метки сообщений.
        /// </summary>
        /// <summary>
        /// Устанавливает конфигуратор конечной точки подписки по умолчанию.
        /// </summary>
        /// <param name="endpointBuilder">
        /// </param>
        void UseSubscriptionEndpointBuilder(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder);


        /// <summary>
        /// Sets excluded incoming message headers
        /// </summary>
        /// <param name="excludedHeaders">Excluded headers</param>
        void SetExcludedIncomingHeaders(IEnumerable<string> excludedHeaders);

        /// <summary>
        /// Sets the bus message headers storage
        /// </summary>
        /// <param name="storage"></param>
        void UseIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage);

        /// <summary>
        /// Sets the bus connection string provider
        /// </summary>
        /// <param name="provider"></param>
        void UseConnectionStringProvider(IConnectionStringProvider provider);

        /// <summary>
        /// Experimental code, remove or refactor
        /// </summary>
        void SetExperimentalProducerSelector();
    }
}
