namespace Contour
{
    using System;

    /// <summary>
    ///   »сключение возникающее в случае неготовности шины к работе (например неполна€ инициализаци€).
    /// </summary>
    public class BusNotReadyException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusNotReadyException"/>.
        /// </summary>
        internal BusNotReadyException()
        {
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusNotReadyException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        internal BusNotReadyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="BusNotReadyException"/>.
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

        #endregion
    }
}
