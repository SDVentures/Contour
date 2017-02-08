namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionPool : ConnectionPool<IRabbitConnection>
    {
        public RabbitConnectionPool(RabbitBus bus, int maxSize) : base(maxSize)
        {
            this.Provider = new RabbitConnectionProvider(bus);
        }
    }
}