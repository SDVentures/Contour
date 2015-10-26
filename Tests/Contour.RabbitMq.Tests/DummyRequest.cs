namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// Запрос для интеграционных тестов.
    /// </summary>
    public class DummyRequest
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DummyRequest"/>.
        /// </summary>
        /// <param name="num">
        /// Данные запроса.
        /// </param>
        public DummyRequest(int num)
        {
            this.Num = num;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DummyRequest"/>.
        /// </summary>
        protected DummyRequest()
        {
        }

        /// <summary>
        /// Данные запроса.
        /// </summary>
        public int Num { get; private set; }
    }
}
