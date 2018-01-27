namespace Contour.Caching
{
    using Contour.Receiving;

    public class CachingContext<T> : IConsumingContext<T>
        where T : class
    {
        private readonly IConsumingContext<T> context;

        private readonly CacheConfiguration cacheConfiguration;

        public CachingContext(IConsumingContext<T> context, CacheConfiguration cacheConfiguration)
        {
            this.context = context;
            this.cacheConfiguration = cacheConfiguration;
        }

        /// <inheritdoc />
        public IBusContext Bus => this.context.Bus;

        /// <inheritdoc />
        public Message<T> Message => this.context.Message;

        /// <inheritdoc />
        public bool CanReply => this.context.CanReply;

        /// <inheritdoc />
        public void Accept()
        {
            this.context.Accept();
        }

        /// <inheritdoc />
        public void Forward(MessageLabel label)
        {
            this.context.Forward(label);
        }

        /// <inheritdoc />
        public void Forward(string label)
        {
            this.context.Forward(label);
        }

        /// <inheritdoc />
        public void Forward<TOut>(MessageLabel label, TOut payload = default(TOut)) where TOut : class
        {
            this.context.Forward(label, payload);
        }

        /// <inheritdoc />
        public void Forward<TOut>(string label, TOut payload = default(TOut)) where TOut : class
        {
            this.context.Forward(label, payload);
        }

        /// <inheritdoc />
        public void Reject(bool requeue)
        {
            this.context.Reject(requeue);
        }

        /// <inheritdoc />
        public void Reply<TResponse>(TResponse response, Expires expires = null) where TResponse : class
        {
            if ((this.cacheConfiguration.Enabled ?? false) && this.cacheConfiguration.Ttl != null)
            {
                this.cacheConfiguration.Cache.Set(this.context.Message, response, this.cacheConfiguration.Ttl.Value);
            }

            this.context.Reply(response, expires);
        }
    }
}