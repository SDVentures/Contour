namespace Contour.Transport.RabbitMQ.Internal
{
    using global::RabbitMQ.Client;

    /// <summary>RabbitMQ message properties.</summary>
    internal class RabbitMessageProperties
    {
        /// <summary>Gets or sets the message timestamp.</summary>
        public AmqpTimestamp Timestamp { get; set; }
    }
}