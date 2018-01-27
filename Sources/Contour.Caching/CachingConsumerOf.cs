namespace Contour.Caching
{
    using Contour.Receiving;
    using Contour.Receiving.Consumers;

    public class CachingConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        private readonly IConsumerOf<T> consumer;

        private readonly CacheConfiguration cacheConfiguration;
        
        public CachingConsumerOf(IConsumerOf<T> consumer, CacheConfiguration cacheConfiguration)
        {
            this.consumer = consumer;
            this.cacheConfiguration = cacheConfiguration;
        }

        public void Handle(IConsumingContext<T> context)
        {
            var message = context.Message;

            if (!this.CanReplyAndExistsCacheConfig(context))
            {
                this.consumer.Handle(context);
                return;
            }

            if (this.cacheConfiguration.Cache.ContainsKey(message))
            {
                context.Reply(this.cacheConfiguration.Cache[message]);
                return;
            }

            var cachingContext = new CachingContext<T>(context, this.cacheConfiguration);

            this.consumer.Handle(cachingContext);
        }

        private bool CanReplyAndExistsCacheConfig(IConsumingContext<T> context)
        {
            return context.CanReply && (this.cacheConfiguration?.Enabled ?? false) && this.cacheConfiguration?.Cache != null && this.cacheConfiguration?.Ttl != null;
        }
    }
}