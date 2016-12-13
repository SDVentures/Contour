using Contour.Receiving;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitListenerConfiguration
    {
        IReceiverConfiguration ReceiverConfiguration { get; }
    }
}