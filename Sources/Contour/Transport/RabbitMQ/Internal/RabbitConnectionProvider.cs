namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionProvider : IRabbitConnectionProvider
    {
        private readonly RabbitBus bus;

        public RabbitConnectionProvider(RabbitBus bus)
        {
            this.bus = bus;
        }

        public IRabbitConnection Create()
        {
            return new RabbitConnection(bus);
        }
    }
}
