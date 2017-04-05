namespace Contour.Receiving
{
    using Contour.Receiving.Consumers;

    /// <summary>
    /// The abstract receiver.
    /// </summary>
    internal abstract class AbstractReceiver : IReceiver
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AbstractReceiver"/>.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        protected AbstractReceiver(IReceiverConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IReceiverConfiguration Configuration { get; private set; }

        public abstract bool IsHealthy { get; }

        /// <summary>
        /// The dispose.
        /// </summary>
        public virtual void Dispose()
        {
            this.Stop();
        }

        /// <summary>
        /// The register consumer.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="consumer">
        /// The consumer.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public abstract void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer) where T : class;

        /// <summary>
        /// The start.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// The stop.
        /// </summary>
        public abstract void Stop();
    }
}
