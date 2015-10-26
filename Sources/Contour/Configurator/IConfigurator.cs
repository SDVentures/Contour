namespace Contour.Configurator
{
    using Contour.Configuration;

    /// <summary>
    ///   Предоставляет функционал по конфигурированию IBus
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Конфигурирует service bus. Устанавливает очереди для отправки сообщений.
        /// </summary>
        /// <param name="endpointName">
        /// </param>
        /// <param name="currentConfiguration">
        /// </param>
        /// <returns>
        /// The <see cref="IBusConfigurator"/>.
        /// </returns>
        IBusConfigurator Configure(string endpointName, IBusConfigurator currentConfiguration);

        /// <summary>
        /// Получить имя сообщение по его ключу
        /// </summary>
        /// <param name="endpointName">
        /// </param>
        /// <param name="key">
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetEvent(string endpointName, string key);

        /// <summary>
        /// The get request config.
        /// </summary>
        /// <param name="endpointName">
        /// The endpoint name.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="IRequestConfiguration"/>.
        /// </returns>
        IRequestConfiguration GetRequestConfig(string endpointName, string key);
    }
}
