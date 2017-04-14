using System;
using System.Runtime.Serialization;

namespace Contour.Transport.RabbitMq
{
    /// <summary>
    /// The unconfirmed message exception is raised when a publisher has not received a confirmation from a RabbitMQ broker which should have been delivered in the Confirm mode. See Publisher Confirms section in https://www.rabbitmq.com/confirms.html
    /// </summary>
    [Serializable]
    public class UnconfirmedMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnconfirmedMessageException"/> class.
        /// </summary>
        public UnconfirmedMessageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnconfirmedMessageException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public UnconfirmedMessageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnconfirmedMessageException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="inner">
        /// The inner.
        /// </param>
        public UnconfirmedMessageException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnconfirmedMessageException"/> class.
        /// </summary>
        /// <param name="info">
        /// The serialization info.
        /// </param>
        /// <param name="context">
        /// The streaming context.
        /// </param>
        protected UnconfirmedMessageException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// The message sequence number provided by the publisher.
        /// </summary>
        public ulong SequenceNumber { get; set; }
    }
}