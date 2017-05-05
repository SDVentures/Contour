namespace Contour.Transport.RabbitMq.Internal
{
    internal class RabbitConnectionPool : ConnectionPool<IRabbitConnection>
    {
        public RabbitConnectionPool(IBusContext context, IPayloadConverterResolver payloadConverterResolver)
        {
            this.Provider = new RabbitConnectionProvider(context, payloadConverterResolver);
        }
    }
}