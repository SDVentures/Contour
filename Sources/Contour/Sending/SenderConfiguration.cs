using System;

using Contour.Helpers;
using Contour.Helpers.CodeContracts;
using Contour.Receiving;

namespace Contour.Sending
{
    using Configuration;

    /// <summary>
    /// Конфигурация отправителя сообщений.
    /// Используется для конфигурирования вариантов односторонних коммуникаций и коммуникаций в формате запрос-ответ.
    /// </summary>
    internal class SenderConfiguration : ISenderConfiguration, ISenderConfigurator
    {
        /// <summary>
        /// Настройки получателя сообщений.
        /// Используются для конфигурирования получения ответа на запрос.
        /// </summary>
        private readonly ReceiverOptions receiverOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SenderConfiguration"/> class. 
        /// </summary>
        /// <param name="label">
        /// Метка отправляемых сообщений.
        /// </param>
        /// <param name="parentOptions">
        /// Настройки отправителя.
        /// </param>
        /// <param name="receiverOptions">
        /// Настройки получателя (для ответных сообщений).
        /// </param>
        public SenderConfiguration(MessageLabel label, SenderOptions parentOptions, ReceiverOptions receiverOptions)
        {
            this.receiverOptions = receiverOptions;
            Requires.Format(MessageLabel.IsValidLabel(label.Name), "label");
            this.Label = label;

            this.Options = (SenderOptions)parentOptions.Derive();
        }

        /// <summary>
        /// Псевдоним метки сообщений.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Конфигурация получателя ответных сообщений.
        /// </summary>
        public IReceiverConfiguration CallbackConfiguration { get; private set; }

        /// <summary>
        /// Метка отправляемых сообщений.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        /// Настройки отправителя сообщений.
        /// </summary>
        public SenderOptions Options { get; private set; }

        /// <summary>
        /// Нужен ли обратный вызов для отправки сообщений.
        /// </summary>
        public bool RequiresCallback
        {
            get
            {
                return this.CallbackConfiguration != null;
            }
        }

        /// <summary>
        /// Дополняет конфигурацию.
        /// </summary>
        /// <param name="routeResolverBuilder">Построитель вычислителя маршрута.</param>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator ConfiguredWith(Func<IRouteResolverBuilder, IRouteResolver> routeResolverBuilder)
        {
            this.Options.RouteResolverBuilder = routeResolverBuilder;

            return this;
        }

        /// <summary>
        /// Устанавливает необходимость сохранять сообщения на диске.
        /// </summary>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator Persistently()
        {
            this.Options.Persistently = true;

            return this;
        }

        public ISenderConfiguration Delayed()
        {
            this.Options.Delayed = true;
            return this;
        }

        public ISenderConfigurator Direct()
        {
            this.Options.Direct = true;

            return this;
        }

        /// <summary>
        /// Проверяет корректность конфигурации.
        /// </summary>
        public void Validate()
        {
            if (this.Label.IsEmpty)
            {
                throw new BusConfigurationException("Can't send using Empty label.");
            }
        }

        /// <summary>
        /// Устанавливает псевдоним для метки сообщения.
        /// </summary>
        /// <param name="alias">Псевдоним метки сообщения.</param>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithAlias(string alias)
        {
            string tempAlias = MessageLabel.AliasPrefix + alias;
            Requires.Format(MessageLabel.IsValidAlias(tempAlias), "alias");

            this.Alias = tempAlias;

            return this;
        }

        public ISenderConfigurator WithCallbackConnectionString(string connectionString)
        {
            this.receiverOptions.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Устанавливает конечную точку обратного вызова для получения ответных сообщений.
        /// </summary>
        /// <param name="callbackEndpointBuilder">Построитель конечной точки для ответных сообщений.</param>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithCallbackEndpoint(Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint> callbackEndpointBuilder)
        {
            IReceiverConfigurator configurator = new ReceiverConfiguration(MessageLabel.Any, this.receiverOptions).WithEndpoint(callbackEndpointBuilder);

            this.CallbackConfiguration = (IReceiverConfiguration)configurator;

            return this;
        }

        /// <summary>
        /// Устанавливает необходимость подтверждения отправки сообщения.
        /// </summary>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithConfirmation()
        {
            this.Options.ConfirmationIsRequired = true;

            return this;
        }

        /// <summary>
        /// Устанавливает время ожидания подтверждения получения сообщения.
        /// </summary>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithConfirmationTimeout(TimeSpan timeout)
        {
            this.Options.ConfirmationTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Устанавливает конечную точку обратного вызова по умолчанию для получения сообщений.
        /// </summary>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithDefaultCallbackEndpoint()
        {
            return this.WithCallbackEndpoint(seb => seb.UseDefaultTempReplyEndpoint(this));
        }

        /// <summary>
        /// Устанавливает временной интервал за который должен быть получен ответ на запрос.
        /// </summary>
        /// <param name="timeout">Временной интервал за который должен быть получен ответ на запрос.</param>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithRequestTimeout(TimeSpan? timeout)
        {
            this.Options.RequestTimeout = timeout;

            return this;
        }

        /// <summary>
        /// Устанавливает время жизни отправляемого сообщения.
        /// </summary>
        /// <param name="ttl">Время жизни отправляемого сообщения.</param>
        /// <returns>Конфигурация отправителя.</returns>
        public ISenderConfigurator WithTtl(TimeSpan ttl)
        {
            this.Options.Ttl = ttl;

            return this;
        }

        /// <summary>
        /// Устанавливает хранилище заголовков входящего сообщения.
        /// </summary>
        /// <param name="storage">Хранилище заголовков входящего сообщения.</param>
        /// <returns>Конфигуратор отправителя с установленным хранилище заголовков входящего сообщения.</returns>
        public ISenderConfigurator WithIncomingMessageHeaderStorage(IIncomingMessageHeaderStorage storage)
        {
            this.Options.IncomingMessageHeaderStorage = new Maybe<IIncomingMessageHeaderStorage>(storage);

            return this;
        }
    }
}
