namespace Contour.Caching
{
    using System;
    using System.Threading;

    public class ThreadSafeCache : ICache
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private readonly ICache innerCache;

        public ThreadSafeCache(ICache innerCache)
        {
            this.innerCache = innerCache;
        }

        public object this[IMessage key]
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.innerCache[key];
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }

        public bool ContainsKey(IMessage key)
        {
            this.locker.EnterReadLock();
            try
            {
                return this.innerCache.ContainsKey(key);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        public void Set(IMessage key, object value, TimeSpan ttl)
        {
            this.locker.EnterWriteLock();
            try
            {
                this.innerCache.Set(key, value, ttl);
            }
            finally
            {
                this.locker.EnterWriteLock();
            }
        }
    }
}