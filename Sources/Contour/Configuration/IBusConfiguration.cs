using System.Collections.Generic;
using Contour.Filters;
using Contour.Serialization;

namespace Contour.Configuration
{
    /// <summary>
    ///   Конфигурация клиента шины.
    /// </summary>
    public interface IBusConfiguration
    {
        /// <summary>
        ///   Строка подключения к транспорту (брокеру).
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        IEnumerable<IMessageExchangeFilter> Filters { get; }

        /// <summary>
        ///   Обработчик метки сообщений.
        /// </summary>
        IMessageLabelHandler MessageLabelHandler { get; }

        /// <summary>
        ///   Сериализатор сообщений.
        /// </summary>
        IPayloadConverter Serializer { get; }

        /// <summary>
        /// Incoming headers that are excluded from copying to outgoing message
        /// </summary>
        IEnumerable<string> ExcludedIncomingHeaders { get; }
    }
}
