using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        ReadOnlyCollection<IPayloadConverter> Converters { get; }
    }
}
