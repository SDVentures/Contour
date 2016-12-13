using Contour.Receiving;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitListenerConfiguration : IRabbitListenerConfiguration
    {
        public RabbitListenerConfiguration(IReceiverConfiguration configuration)
        {
            this.ReceiverConfiguration = configuration;
        }

        public IReceiverConfiguration ReceiverConfiguration { get; }
    }
}