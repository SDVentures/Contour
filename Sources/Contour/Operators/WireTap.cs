namespace Contour.Operators
{
    /// <summary>
    /// Оператор, который позволяет инспектировать сообщения передаваемые в шине (реализация шаблона <see href="http://www.eaipatterns.com/WireTap.html"/>).
    /// </summary>
    public class WireTap : RecipientList
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WireTap"/>.
        /// </summary>
        /// <param name="messageLabel">Метка сообщения, куда надо перенаправить входящее сообщение.</param>
        public WireTap(MessageLabel messageLabel)
            : base(message => new[] { messageLabel, message.Label })
        {
        }
    }
}
