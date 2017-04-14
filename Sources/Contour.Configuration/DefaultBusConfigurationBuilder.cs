using Contour.Serialization;

namespace Contour.Configuration
{
    /// <summary>
    /// Строитель конфигурации шины с настройками по умолчанию.
    /// </summary>
    internal static class DefaultBusConfigurationBuilder
    {
        /// <summary>
        /// Строит конфигурацию шины.
        /// </summary>
        /// <returns>
        /// Конфигурация шины с настройки по умолчанию.
        /// </returns>
        public static BusConfiguration Build()
        {
            var c = new BusConfiguration();

            c.UseRabbitMq();
            c.UsePayloadConverter(new JsonNetPayloadConverter());
            return c;
        }
    }
}
