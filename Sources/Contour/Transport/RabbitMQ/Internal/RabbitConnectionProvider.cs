using Common.Logging;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RabbitConnectionProvider : IConnectionProvider<IRabbitConnection>
    {
        private readonly ILog logger = LogManager.GetLogger<RabbitConnectionProvider>();
        private readonly IEndpoint endpoint;
        private readonly IBusContext context;
        private readonly bool asyncConsuming;

        public RabbitConnectionProvider(IBusContext context, bool asyncConsuming)
        {
            this.endpoint = context.Endpoint;
            this.context = context;
            this.asyncConsuming = asyncConsuming;
        }

        public IRabbitConnection Create(string connectionString)
        {
            this.logger.Trace($"Creating a new connection for endpoint [{this.endpoint}] at [{connectionString}]");
            return new RabbitConnection(this.endpoint, connectionString, this.context, asyncConsuming);
        }
    }
}
