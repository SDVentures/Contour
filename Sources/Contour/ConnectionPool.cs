using System;
using System.Collections.Concurrent;

namespace Contour
{
    internal abstract class ConnectionPool<TConnection> : IConnectionPool<TConnection> where TConnection : IConnection
    {
        private readonly ConcurrentDictionary<int, TConnection> connections = new ConcurrentDictionary<int, TConnection>();
        protected IConnectionProvider<TConnection> Provider;

        protected ConnectionPool(int maxSize)
        {
            MaxSize = maxSize <= 0 ? int.MaxValue : maxSize;
        }

        public int MaxSize { get; }

        public int Count => connections.Count;

        public TConnection Get()
        {
            var random = new Random();
            var key = Count < MaxSize ? Count : random.Next(MaxSize);
            
            return connections.GetOrAdd(key, i => Provider.Create());
        }
    }
}