namespace Contour.Configuration
{
    using System;

    /// <summary>
    ///   Компонент клиента шины сообщений, существующий в течение срока жизни самого клиента.
    /// </summary>
    public interface IBusComponent : IDisposable
    {
        /// <summary>
        ///   Запуск компонента.
        /// </summary>
        void Start();

        /// <summary>
        ///   Остановка компонента.
        /// </summary>
        void Stop();

        /// <summary>
        /// Проверка работоспособности компонента.
        /// </summary>
        bool IsHealthy { get; }
    }
}
