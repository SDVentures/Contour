namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The foo message.
    /// </summary>
    public class FooMessage
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FooMessage"/>.
        /// </summary>
        /// <param name="num">
        /// Данные сообщения.
        /// </param>
        public FooMessage(int num)
        {
            this.Num = num;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FooMessage"/>.
        /// </summary>
        protected FooMessage()
        {
        }

        /// <summary>
        /// Данные сообщения.
        /// </summary>
        public int Num { get; private set; }
    }
}
