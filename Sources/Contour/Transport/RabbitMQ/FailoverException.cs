using System;
using System.Runtime.Serialization;

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// An exception is raised if the bus was unable to handle a message in a fault tolerant manner, ex. publish attempts exhausted.
    /// </summary>
    [Serializable]
    public class FailoverException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverException"/> class.
        /// </summary>
        public FailoverException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public FailoverException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="inner">
        /// The inner.
        /// </param>
        public FailoverException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverException"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected FailoverException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the attempts taken to complete an operation
        /// </summary>
        public int Attempts { get; set; }
    }
}