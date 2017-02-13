using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionProvider : IConnectionProvider<IRabbitConnection>
    {
        private readonly ILog logger = LogManager.GetLogger<RabbitConnectionProvider>();
        private readonly RabbitBus bus;

        public RabbitConnectionProvider(RabbitBus bus)
        {
            this.bus = bus;
        }
        
        public IRabbitConnection Create()
        {
            this.logger.Trace($"Creating a new connection: [{typeof(IRabbitConnection).Name}]");
            return new RabbitConnection(this.bus);
        }
    }
}
