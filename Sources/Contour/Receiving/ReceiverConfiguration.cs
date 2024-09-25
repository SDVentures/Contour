using System;

using Contour.Helpers;
using Contour.Receiving.Consumers;
using Contour.Validation;

namespace Contour.Receiving
{
    /// <summary>
    /// Конфигурация получателя.
    /// </summary>
    internal class ReceiverConfiguration : IReceiverConfiguration, IReceiverConfigurator
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ReceiverConfiguration"/>.
        /// </summary>
        /// <param name="label">Метка получаемого сообщения.</param>
        /// <param name="parentOptions">Настройки получателя.</param>
        public ReceiverConfiguration(MessageLabel label, ReceiverOptions parentOptions)
        {
            this.Label = label;
            this.Options = (ReceiverOptions)parentOptions.Derive();
        }

        /// <summary>
        /// Псевдоним получаемого сообщения.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Метка получаемого сообщения.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        /// Настройки получателя.
        /// </summary>
        public ReceiverOptions Options { get; private set; }

        /// <summary>
        /// Регистратор получателя.
        /// </summary>
        public Action<IReceiver> ReceiverRegistration { get; protected internal set; }

        /// <summary>
        /// Механизм проверки получаемых сообщений.
        /// </summary>
        public IMessageValidator Validator { get; private set; }

        /// <summary>
        /// Возвращает типизированный конфигуратор получателя.
        /// </summary>
        /// <typeparam name="T">Тип конфигуратора получателя.</typeparam>
        /// <returns>Конфигуратор получателя.</returns>
        public IReceiverConfigurator<T> As<T>() where T : class
        {
            return new TypedReceiverConfigurationDecorator<T>(this);
        }

        /// <summary>
        /// Регистрирует стратегию обработки сообщений, получение которых завершилось провалом.
        /// </summary>
        /// <param name="failedDeliveryStrategy">Стратегия обработки сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений.</returns>
        public IReceiverConfigurator OnFailed(IFailedDeliveryStrategy failedDeliveryStrategy)
        {
            this.Options.FailedDeliveryStrategy = failedDeliveryStrategy.Maybe();

            return this;
        }

        /// <summary>Регистрирует обработчик сообщений, получение которых завершилось провалом.</summary>
        /// <param name="failedDeliveryHandler">Обработчик сообщений, получение которых завершилось провалом.</param>
        /// <returns>Конфигуратор получателя с установленной стратегией обработки сообщений</returns>
        public IReceiverConfigurator OnFailed(Action<IFailedConsumingContext> failedDeliveryHandler)
        {
            return this.OnFailed(new LambdaFailedDeliveryStrategy(failedDeliveryHandler));
        }

        /// <summary>Регистрирует фабрику обработчиков входящего сообщения.</summary>
        /// <param name="consumerFactoryFunc">Фабрика обработчиков входящих сообщений.</param>
        /// <typeparam name="T">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T> ReactWith<T>(Func<IConsumerOf<T>> consumerFactoryFunc) where T : class
        {
            this.ReceiverRegistration = l => l.RegisterConsumer(this.Label, new FactoryConsumerOf<T>(consumerFactoryFunc));

            return new TypedReceiverConfigurationDecorator<T>(this);
        }

        /// <summary>Регистрирует обработчик входящего сообщения.</summary>
        /// <param name="consumer">Обработчик входящего сообщения.</param>
        /// <typeparam name="T">Тип обрабатываемого сообщения.</typeparam>
        /// <returns>Конфигуратор получателя с установленным обработчиком входящих сообщений.</returns>
        public IReceiverConfigurator<T> ReactWith<T>(IConsumer<T> consumer) where T : class
        {
            this.ReceiverRegistration = l => l.RegisterConsumer(this.Label, consumer);

            return new TypedReceiverConfigurationDecorator<T>(this);
        }

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки сообщения.
        /// </summary>
        /// <returns>
        /// Конфигуратор получателя с явным подтверждением успешной обработки сообщения.
        /// </returns>
        public IReceiverConfigurator RequiresAccept()
        {
            this.Options.AcceptIsRequired = true;

            return this;
        }

        /// <summary>
        /// Проверяет конфигурацию.
        /// </summary>
        public virtual void Validate()
        {
            if (this.Label.IsEmpty)
            {
                throw new InvalidOperationException("Can't receive using Empty label.");
            }
        }

        /// <summary>
        /// Регистрирует механизм проверки входящего сообщения.
        /// </summary>
        /// <param name="validator">Механизм проверки входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным механизмом проверки входящих сообщений.</returns>
        public IReceiverConfigurator WhenVerifiedBy(IMessageValidator validator)
        {
            this.Validator = validator;

            return this;
        }

        /// <summary>
        /// Устанавливает псевдоним входящих сообщений.
        /// </summary>
        /// <param name="alias">Псевдоним входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным псевдонимом входящих сообщений.</returns>
        public IReceiverConfigurator WithAlias(string alias)
        {
            this.Alias = MessageLabel.AliasPrefix + alias;

            return this;
        }

        /// <summary>
        /// Регистрирует построитель порта, по которому проходит получение входящих сообщений.
        /// </summary>
        /// <param name="endpointBuilder">Построитель порта входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным построителем порта входящих сообщений.</returns>
        public IReceiverConfigurator WithEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> endpointBuilder)
        {
            this.Options.EndpointBuilder = endpointBuilder;

            return this;
        }

        /// <summary>
        /// Устанавливает количество одновременных обработчиков входящих сообщений.
        /// </summary>
        /// <param name="parallelismLevel">Количество одновременных обработчиков входящих сообщений.</param>
        /// <returns>Конфигуратор получателя с установленным количеством одновременных обработчиков входящих сообщений.</returns>
        public IReceiverConfigurator WithParallelismLevel(uint parallelismLevel)
        {
            this.Options.ParallelismLevel = parallelismLevel;

            return this;
        }

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор получателя с установленным хранилищем заголовков.</returns>
        public IReceiverConfigurator WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage)
        {
            this.Options.IncomingMessageHeaderStorage = new Maybe<IIncomingMessageHeaderStorage>(storage);

            return this;
        }


        public IReceiverConfigurator WithQueueLimit(int queueLimit)
        {
            this.Options.QueueLimit = queueLimit;

            return this;
        }

        public IReceiverConfigurator WithQueueMaxLengthBytes(int bytes)
        {
            this.Options.QueueMaxLengthBytes = bytes;

            return this;
        }

        /// <inheritdoc />
        public IReceiverConfigurator Delayed()
        {
            this.Options.Delayed = true;

            return this;
        }

        public IReceiverConfigurator Direct(string id)
        {
            this.Options.Direct = true;
            this.Options.DirectId = id;

            return this;
        }
    }
}
