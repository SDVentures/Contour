using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Sending
{
    public class DynamicRouteResolver : IRouteResolver
    {
        private readonly IRouteResolverBuilder routeResolverBuilder;

        private readonly HashSet<string> createdExchangesCache = new HashSet<string>();

        private readonly ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim();

        public DynamicRouteResolver(IRouteResolverBuilder routeResolverBuilder)
        {
            this.routeResolverBuilder = routeResolverBuilder;
        }

        public IRoute Resolve(IEndpoint endpoint, MessageLabel label)
        {
            try
            {
                if (this.createdExchangesCache.Contains(label.Name))
                {
                    return new RabbitRoute(label.Name);
                }

                this.slimLock.EnterWriteLock();
                if (this.createdExchangesCache.Contains(label.Name))
                {
                    return new RabbitRoute(label.Name);
                }

                this.routeResolverBuilder.Topology.Declare(Exchange.Named(label.Name).Durable.Fanout);

                this.createdExchangesCache.Add(label.Name);

                return new RabbitRoute(label.Name);
            }
            finally
            {
                if (this.slimLock.IsWriteLockHeld)
                {
                    this.slimLock.ExitWriteLock();
                }
            }
        }
    }
}
