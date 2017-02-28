namespace Contour
{
    using System;
    using Configuration;

    /// <summary>
    ///   Фабрика для создания клиента шины сообщений на основе конфигурации.
    /// </summary>
    public class BusFactory
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
        public IBus Create(Action<IBusConfigurator> configure, bool autoStart = true)
        {
            var config = DefaultBusConfigurationBuilder.Build();

            configure(config);

            config.Validate();

            var bus = config.BusFactoryFunc(config);

            if (autoStart)
            {
                bus.Start();
            }

            return bus;
        }
    }
}
