using System;

using Contour.Receiving.Consumers;
using Contour.Validation;

namespace Contour.Receiving
{
    /// <summary>
    /// Типизированный конфигуратор получателя сообщений.
    /// </summary>
    /// <typeparam name="T">Тип получаемого сообщения.</typeparam>
    internal class TypedReceiverConfigurationDecorator<T> : IReceiverConfigurator<T>
        where T : class
    {
        /// <summary>
        /// Конфигурация получателя сообщений.
        /// </summary>
        private readonly ReceiverConfiguration configuration;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TypedReceiverConfigurationDecorator{T}"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация получателя сообщений.</param>
        public TypedReceiverConfigurationDecorator(ReceiverConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Конфигурация получателя сообщений.
        /// </summary>
        public ReceiverConfiguration Configuration => this.configuration;

        /// <summary>
        /// Возвращает типизированный конфигуратор получателя.
        /// </summary>
        /// <typeparam name="T1">Тип конфигуратора получателя.</typeparam>
        /// <returns>Конфигуратор получателя.</returns>
        public IReceiverConfigurator<T1> As<T1>() where T1 : class
        {
            return this.configuration.As<T1>();
        }

        /// <summary>
        /// Регистрирует стратегию обработки сообщений, получение которых завершилось провалом.
        /// </summary>
        /// <param name="failedDeliveryStrategy">Стратегия обработки сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений.</returns>
        public IReceiverConfigurator OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy)
        {
            return this.configuration.OnFailed(failedDeliveryStrategy);
        }

        /// <summary>Регистрирует обработчик сообщений, получение которых завершилось провалом.</summary>
        /// <param name="failedDeliveryHandler">Обработчик сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений</returns>
        public IReceiverConfigurator OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler)
        {
            return this.configuration.OnFailed(failedDeliveryHandler);
        }

        /// <summary>Регистрирует фабрику обработчиков входящего сообщения.</summary>
        /// <param name="consumerFactoryFunc">Фабрика обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T> ReactWith(Func<IConsumerOf<T>> consumerFactoryFunc)
        {
            this.configuration.ReactWith(consumerFactoryFunc);

            return this;
        }

        /// <summary>Регистрирует обработчик входящего сообщения.</summary>
        /// <param name="consumer">Обработчик входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T> ReactWith(IConsumerOf<T> consumer)
        {
            this.configuration.ReactWith(consumer);

            return this;
        }

        /// <summary>Регистрирует фабрику обработчиков входящего сообщения.</summary>
        /// <param name="consumerFactoryFunc">Фабрика обработчиков входящих сообщений.</param>
        /// <typeparam name="T1">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T1> ReactWith<T1>(Func<IConsumerOf<T1>> consumerFactoryFunc) where T1 : class
        {
            return this.configuration.ReactWith(consumerFactoryFunc);
        }

        /// <summary>Регистрирует обработчик входящего сообщения.</summary>
        /// <param name="consumer">Обработчик входящего сообщения.</param>
        /// <typeparam name="T1">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T1> ReactWith<T1>(IConsumer<T1> consumer) where T1 : class
        {
            return this.configuration.ReactWith(consumer);
        }

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки сообщения.
        /// </summary>
        /// <returns>
        /// Конфигуратор получателя с явным подтверждением успешной обработки сообщения.
        /// </returns>
        public IReceiverConfigurator RequiresAccept()
        {
            return this.configuration.RequiresAccept();
        }

        /// <summary>
        /// Регистрирует механизм проверки входящего сообщения.
        /// </summary>
        /// <param name="validator">Механизм проверки входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным механизмом проверки входящих сообщений.</returns>
        public IReceiverConfigurator WhenVerifiedBy(IMessageValidator validator)
        {
            this.configuration.WhenVerifiedBy(validator);

            return this;
        }

        /// <summary>
        /// Регистрирует механизм проверки входящего сообщения.
        /// </summary>
        /// <param name="validator">Механизм проверки входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным механизмом проверки входящих сообщений.</returns>
        public IReceiverConfigurator<T> WhenVerifiedBy(IMessageValidatorOf<T> validator)
        {
            this.configuration.WhenVerifiedBy(validator);

            return this;
        }

        /// <summary>
        /// Устанавливает псевдоним входящих сообщений.
        /// </summary>
        /// <param name="alias">Псевдоним входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным псевдонимом входящих сообщений.</returns>
        public IReceiverConfigurator<T> WithAlias(string alias)
        {
            this.configuration.WithAlias(alias);

            return this;
        }

        /// <summary>
        /// Регистрирует построитель порта, по которому проходит получение входящих сообщений.
        /// </summary>
        /// <param name="endpointBuilder">Построитель порта входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным построителем порта входящих сообщений.</returns>
        public IReceiverConfigurator WithEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder)
        {
            return this.configuration.WithEndpoint(endpointBuilder);
        }

        /// <summary>
        /// Устанавливает количество одновременных обработчиков входящих сообщений.
        /// </summary>
        /// <param name="parallelismLevel">Количество одновременных обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным количеством одновременных обработчиков входящих сообщений.</returns>
        public IReceiverConfigurator WithParallelismLevel(uint parallelismLevel)
        {
            return this.configuration.WithParallelismLevel(parallelismLevel);
        }

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным хранилищем заголовков.</returns>
        IReceiverConfigurator IReceiverConfigurator<T>.WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage)
        {
            this.configuration.WithIncomingMessageHeaderStorage(storage);

            return this;
        }

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным хранилищем заголовков.</returns>
        IReceiverConfigurator IReceiverConfigurator.WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage)
        {
            this.configuration.WithIncomingMessageHeaderStorage(storage);

            return this;
        }

        /// <summary>
        /// Регистрирует стратегию обработки сообщений, получение которых завершилось провалом.
        /// </summary>
        /// <param name="failedDeliveryStrategy">Стратегия обработки сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений.</returns>
        IReceiverConfigurator<T> IReceiverConfigurator<T>.OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy)
        {
            this.configuration.OnFailed(failedDeliveryStrategy);

            return this;
        }

        /// <summary>Регистрирует обработчик сообщений, получение которых завершилось провалом.</summary>
        /// <param name="failedDeliveryHandler">Обработчик сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений</returns>
        IReceiverConfigurator<T> IReceiverConfigurator<T>.OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler)
        {
            this.configuration.OnFailed(failedDeliveryHandler);

            return this;
        }

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки сообщения.
        /// </summary>
        /// <returns>
        /// Конфигуратор получателя с явным подтверждением успешной обработки сообщения.
        /// </returns>
        IReceiverConfigurator<T> IReceiverConfigurator<T>.RequiresAccept()
        {
            this.configuration.RequiresAccept();

            return this;
        }

        /// <summary>
        /// Устанавливает псевдоним входящих сообщений.
        /// </summary>
        /// <param name="alias">Псевдоним входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным псевдонимом входящих сообщений.</returns>
        IReceiverConfigurator IReceiverConfigurator.WithAlias(string alias)
        {
            return this.WithAlias(alias);
        }

        /// <summary>
        /// Регистрирует построитель порта, по которому проходит получение входящих сообщений.
        /// </summary>
        /// <param name="endpointBuilder">Построитель порта входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным построителем порта входящих сообщений.</returns>
        IReceiverConfigurator<T> IReceiverConfigurator<T>.WithEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder)
        {
            this.configuration.WithEndpoint(endpointBuilder);

            return this;
        }

        /// <summary>
        /// Устанавливает количество одновременных обработчиков входящих сообщений.
        /// </summary>
        /// <param name="parallelismLevel">Количество одновременных обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным количеством одновременных обработчиков входящих сообщений.</returns>
        IReceiverConfigurator<T> IReceiverConfigurator<T>.WithParallelismLevel(uint parallelismLevel)
        {
            this.configuration.WithParallelismLevel(parallelismLevel);

            return this;
        }

        public IReceiverConfigurator WithQueueLimit(int queueLimit)
        {
            return this.configuration.WithQueueLimit(queueLimit);
        }

        public IReceiverConfigurator WithQueueMaxLengthBytes(int maxLengthBytes)
        {
            return this.configuration.WithQueueMaxLengthBytes(maxLengthBytes);
        }

        /// <inheritdoc />
        public IReceiverConfigurator Delayed()
        {
            this.configuration.Delayed();

            return this;
        }

        public IReceiverConfigurator Direct(string id)
        {
            return this.configuration.Direct(id);
        }
    }
}
