namespace Contour
{
    using System;

    /// <summary>
    ///   Исключение возникающее в случае неготовности шины к работе (например неполная инициализация).
    /// </summary>
    public class BusNotReadyException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BusNotReadyException"/>.
        /// </summary>
        internal BusNotReadyException()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BusNotReadyException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        internal BusNotReadyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BusNotReadyException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        internal BusNotReadyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
