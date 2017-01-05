namespace Contour
{
    using System;

    /// <summary>Bus message properties.</summary>
    public class MessageProperties
    {
        /// <summary>Initializes a new instance of the <see cref="MessageProperties"/> class.</summary>
        /// <param name="timestamp">Message timestamp.</param>
        public MessageProperties(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }

        /// <summary>Initializes a new instance of the <see cref="MessageProperties"/> class.</summary>
        public MessageProperties()
        {
        }

        /// <summary>Gets or sets message timestamp.</summary>
        public DateTime Timestamp { get; set; }
    }
}