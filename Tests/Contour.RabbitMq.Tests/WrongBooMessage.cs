using System;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The wrong boo message.
    /// </summary>
    public class WrongBooMessage
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WrongBooMessage"/>.
        /// </summary>
        /// <param name="num">
        /// Данные сообщения.
        /// </param>
        public WrongBooMessage(DateTime num)
        {
            this.Num = num;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WrongBooMessage"/>.
        /// </summary>
        protected WrongBooMessage()
        {
        }

        /// <summary>
        /// Данные сообщения.
        /// </summary>
        public DateTime Num { get; private set; }
    }
}
