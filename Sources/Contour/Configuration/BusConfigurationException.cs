namespace Contour.Configuration
{
    using System;

    /// <summary>
    ///   »сключение св€занное с некорректной конфигурацией шины.
    /// </summary>
    public class BusConfigurationException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusConfigurationException"/>.
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
