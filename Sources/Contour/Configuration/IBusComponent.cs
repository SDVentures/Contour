namespace Contour.Configuration
{
    using System;

    /// <summary>
    ///   Компонент клиента шины сообщений, существующий в течение срока жизни самого клиента.
    /// </summary>
    public interface IBusComponent : IDisposable
    {
        #region Public Methods and Operators

        /// <summary>
        ///   Запуск компонента.
        /// </summary>
        void Start();

        /// <summary>
        ///   Остановка компонента.
        /// </summary>
        void Stop();

        #endregion

        /// <summary>
        /// Проверка работоспособности компонента.
        /// </summary>
        bool IsHealthy { get; }
    }
}
