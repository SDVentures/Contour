using System;

namespace Contour.Transport.RabbitMq
{
    /// <summary>
    /// The rabbit exception.
    /// </summary>
    public class RabbitException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public RabbitException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public RabbitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
