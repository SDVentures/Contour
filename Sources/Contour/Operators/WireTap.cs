namespace Contour.Operators
{
    /// <summary>
    /// ќператор, который позвол€ет инспектировать сообщени€ передаваемые в шине (реализаци€ шаблона <see href="http://www.eaipatterns.com/WireTap.html"/>).
    /// </summary>
    public class WireTap : RecipientList
    {
        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="WireTap"/>.
        /// </summary>
        /// <param name="messageLabel">ћетка сообщени€, куда надо перенаправить вход€щее сообщение.</param>
        public WireTap(MessageLabel messageLabel)
            : base(message => new[] { messageLabel, message.Label })
        {
        }
    }
}
