namespace Contour.Sending
{
    using System;

    /// <summary>
    /// The message rejected exception.
    /// </summary>
    public class MessageRejectedException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageRejectedException"/>.
        /// </summary>
        public MessageRejectedException()
            : base("Message was rejected.")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageRejectedException"/>.
        /// </summary>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public MessageRejectedException(Exception innerException)
            : base("Message was rejected.", innerException)
        {
        }
    }
}
