using Contour.Receiving;
using Contour.Sending;
using Contour.Validation;

namespace Contour.Configuration
{
    using System.Collections.Generic;

    using Contour.Filters;
    using Contour.Serialization;

    /// <summary>
    ///   Конфигурация клиента шины.
    /// </summary>
    public interface IBusConfiguration
    {
        #region Public Properties

        /// <summary>
        ///   Строка подключения к транспорту (брокеру).
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        IList<IMessageExchangeFilter> Filters { get; }

        /// <summary>
        /// Gets a bus life-cycle handler
        /// </summary>
        IBusLifecycleHandler LifecycleHandler { get; }

        /// <summary>
        ///   Обработчик метки сообщений.
        /// </summary>
        IMessageLabelHandler MessageLabelHandler { get; }

        /// <summary>
        /// Gets a message serializer
        /// </summary>
        IPayloadConverter Serializer { get; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        IEndpoint Endpoint { get; }

        /// <summary>
        /// Gets the receiver configurations.
        /// </summary>
        IList<IReceiverConfiguration> ReceiverConfigurations { get; }

        /// <summary>
        /// Gets the sender configurations.
        /// </summary>
        IList<ISenderConfiguration> SenderConfigurations { get; }

        /// <summary>
        /// Gets the message label resolver.
        /// </summary>
        IMessageLabelResolver MessageLabelResolver { get; }

        /// <summary>
        /// Gets the validator registry.
        /// </summary>
        IMessageValidatorRegistry ValidatorRegistry { get; }

        #endregion
    }
}
