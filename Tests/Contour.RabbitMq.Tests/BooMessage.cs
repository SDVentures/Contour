namespace Contour.RabbitMq.Tests
{
    public class BooMessage
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BooMessage"/>.
        /// </summary>
        /// <param name="num">
        /// Данные сообщения.
        /// </param>
        public BooMessage(int num)
        {
            this.Num = num;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BooMessage"/>.
        /// </summary>
        protected BooMessage()
        {
        }

        /// <summary>
        /// Данные сообщения.
        /// </summary>
        public int Num { get; private set; }
    }
}
