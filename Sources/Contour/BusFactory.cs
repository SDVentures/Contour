namespace Contour
{
    using System;

    using Contour.Configuration;

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
            BusConfiguration config = DefaultBusConfigurationBuilder.Build();

            configure(config);

            config.Validate();

            IBus bus = config.BusFactoryFunc(config);

            if (autoStart)
            {
                bus.Start();
            }

            return bus;
        }
    }
}
