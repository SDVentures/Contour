using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Sending
{
    public class DynamicRouteResolver : IRouteResolver
    {
        private readonly IRouteResolverBuilder routeResolverBuilder;

        private ImmutableHashSet<string> createdExchangesCache;

        private readonly object sync = new object();

        public DynamicRouteResolver(IRouteResolverBuilder routeResolverBuilder)
        {
            this.routeResolverBuilder = routeResolverBuilder;
            this.createdExchangesCache = ImmutableHashSet.Create<string>();
        }

        public IRoute Resolve(IEndpoint endpoint, MessageLabel label)
        {
            if (this.createdExchangesCache.Contains(label.Name))
            {
                return new RabbitRoute(label.Name);
            }

            lock (this.sync)
            {
                if (this.createdExchangesCache.Contains(label.Name))
                {
                    return new RabbitRoute(label.Name);
                }

                this.routeResolverBuilder.Topology.Declare(Exchange.Named(label.Name).Durable.Fanout);

                this.createdExchangesCache = this.createdExchangesCache.Add(label.Name);

                return new RabbitRoute(label.Name);
            }
        }
    }
}
