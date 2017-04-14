﻿using Common.Logging;

namespace Contour.Transport.RabbitMq.Internal
{
    internal class RabbitConnectionProvider : IConnectionProvider<IRabbitConnection>
    {
        private readonly ILog logger = LogManager.GetLogger<RabbitConnectionProvider>();
        private readonly IEndpoint endpoint;
        private readonly IBusContext context;

        public RabbitConnectionProvider(IBusContext context)
        {
            this.endpoint = context.Endpoint;
            this.context = context;
        }

        public IRabbitConnection Create(string connectionString)
        {
            this.logger.Trace($"Creating a new connection for endpoint [{this.endpoint}] at [{connectionString}]");
            return new RabbitConnection(this.endpoint, connectionString, this.context);
        }
    }
}
