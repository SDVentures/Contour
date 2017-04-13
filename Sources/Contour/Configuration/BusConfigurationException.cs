using System;

namespace Contour.Configuration
{
    /// <summary>
    ///   Исключение связанное с некорректной конфигурацией шины.
    /// </summary>
    public class BusConfigurationException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BusConfigurationException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        internal BusConfigurationException(string message)
            : base(message)
        {
        }
    }
}
