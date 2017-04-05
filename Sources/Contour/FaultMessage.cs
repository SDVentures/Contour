// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaultMessage.cs" company="">
//   
// </copyright>
// <summary>
//   The fault message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour
{
    using System;

    /// <summary>
    /// The fault message.
    /// </summary>
    public class FaultMessage
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FaultMessage"/>.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="received">
        /// The received.
        /// </param>
        /// <param name="contentType">
        /// The content type.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public FaultMessage(IMessage message, DateTime received, string contentType, Exception exception = null)
        {
            this.Received = received;
            this.Label = message.Label.Name;
            this.Payload = message.Payload;
            this.ContentType = contentType;
            this.Exception = exception != null ? new FaultException(exception) : null;
        }
        /// <summary>
        /// Gets the content type.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public FaultException Exception { get; private set; }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        public object Payload { get; private set; }

        /// <summary>
        /// Gets the received.
        /// </summary>
        public DateTime Received { get; private set; }
    }
}
