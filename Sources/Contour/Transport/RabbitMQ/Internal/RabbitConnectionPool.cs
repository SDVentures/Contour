namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionPool : ConnectionPool<IRabbitConnection>
    {
        public RabbitConnectionPool(IBusContext context, bool asyncConsuming)
        {
            this.Provider = new RabbitConnectionProvider(context, asyncConsuming);
        }
    }
}
