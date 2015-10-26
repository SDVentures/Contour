namespace Contour.Configuration
{
    using System;

    /// <summary>
    ///   Исключение связанное с некорректной конфигурацией шины.
    /// </summary>
    public class BusConfigurationException : Exception
    {
        #region Constructors and Destructors

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

        #endregion
    }
}
