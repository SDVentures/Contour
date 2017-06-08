namespace Contour.Testing.Plumbing
{
    /// <summary>
    /// Сообщение в очереди брокера.
    /// </summary>
    public class Message
    {
        public MessageProperties Properties { get; set; }
        /// <summary>
        /// Тело сообщения (полезная нагрузка).
        /// </summary>
        public string Payload { get; set; }
    }

    public class MessageProperties
    {
        public string ContentType { get; set; }
    }
}
