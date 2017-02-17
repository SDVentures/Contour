namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionPool : ConnectionPool<IRabbitConnection>
    {
        public RabbitConnectionPool(IBusContext context)
        {
            this.Provider = new RabbitConnectionProvider(context);
        }
    }
}