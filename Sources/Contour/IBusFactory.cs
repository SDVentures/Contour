namespace Contour
{
    using System;
    using Configuration;

    /// <summary>
    /// The BusFactory interface.
    /// </summary>
    public interface IBusFactory
    {
        /// <summary>
        /// Создает экземпляр шины событий
        /// </summary>
        /// <param name="configure">
        /// делегат, используемый для задания настроек шины событий
        /// </param>
        /// <param name="autoStart">
        /// запустить клиент после создания
        /// </param>
        /// <returns>
        /// сконфигурированный экземпляр шины событий
        /// </returns>
        IBus Create(Action<IBusConfigurator> configure, bool autoStart = true);
    }
}