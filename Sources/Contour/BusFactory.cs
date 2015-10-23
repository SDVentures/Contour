namespace Contour
{
    using System;

    using Contour.Configuration;

    /// <summary>
    ///   ‘абрика дл€ создани€ клиента шины сообщений на основе конфигурации.
    /// </summary>
    public class BusFactory
    {
        /// <summary>
        /// —оздает экземпл€р шины событий
        /// </summary>
        /// <param name="configure">
        /// делегат, используемый дл€ задани€ настроек шины событий
        /// </param>
        /// <param name="autoStart">
        /// запустить клиент после создани€
        /// </param>
        /// <returns>
        /// сконфигурированный экземпл€р шины событий
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
