using System;
using Contour.Configuration;
using Contour.Serialization;

namespace Contour.Testing.Plumbing
{
    internal class TestFactory
    {
        public IBus Create(Action<IBusConfigurator> configure, bool autoStart = true)
        {
            var config = new BusConfiguration();
            config.UsePayloadConverter(new JsonNetPayloadConverter());

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
