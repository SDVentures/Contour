namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// Ответ для интеграционных тестов.
    /// </summary>
    public class DummyResponse
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DummyResponse"/>.
        /// </summary>
        /// <param name="num">
        /// Данные ответа.
        /// </param>
        public DummyResponse(int num)
        {
            this.Num = num;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DummyResponse"/>.
        /// </summary>
        protected DummyResponse()
        {
        }

        /// <summary>
        /// Данные ответа.
        /// </summary>
        public int Num { get; private set; }
    }
}
