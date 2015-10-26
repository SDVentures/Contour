namespace Contour.Testing.Plumbing
{
    /// <summary>
    /// Сообщение в очереди брокера.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Тело сообщения (полезная нагрузка).
        /// </summary>
        public string Payload { get; set; }
    }
}
