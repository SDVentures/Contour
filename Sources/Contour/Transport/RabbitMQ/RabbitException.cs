// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RabbitException.cs" company="">
//   
// </copyright>
// <summary>
//   The rabbit exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Transport.RabbitMQ
{
    using System;

    /// <summary>
    /// The rabbit exception.
    /// </summary>
    public class RabbitException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RabbitException"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public RabbitException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RabbitException"/>.
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

        #endregion
    }
}
