using Contour.Receiving;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitCallbackReceiver : RabbitReceiver
    {
        public RabbitCallbackReceiver(RabbitBus bus, IReceiverConfiguration configuration, IConnectionPool<IRabbitConnection> connectionPool)
            : base(bus, configuration, connectionPool)
        {
        }

        protected override void OnListenerCreated(IListener listener)
        {
            // Suppress listener registrations for the callback receiver
        }
    }
}