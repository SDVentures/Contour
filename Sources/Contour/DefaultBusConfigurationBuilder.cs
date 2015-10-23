using Contour.Configuration;
using Contour.Serialization;
using Contour.Transport.RabbitMQ;

namespace Contour
{
    /// <summary>
    /// Построитель конфигурации шины с настройками по умолчанию.
    /// </summary>
    internal static class DefaultBusConfigurationBuilder
    {
        /// <summary>
        /// Строит конфигурацию шины.
        /// </summary>
        /// <returns>
        /// Конфигурация шины с настройкми по умолчнаию.
        /// </returns>
        public static BusConfiguration Build()
        {
            var c = new BusConfiguration();

            c.UseRabbitMq();
            c.UsePayloadConverter(new JsonNetPayloadConverter());

            // c.EnableCaching();
            return c;
        }
    }
}
