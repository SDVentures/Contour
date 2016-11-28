namespace Contour.Transport.RabbitMQ.Internal
{
    using Common.Logging;

    using Contour.Receiving;
    using Contour.Receiving.Consumers;

    /// <summary>
    /// The rabbit receiver.
    /// </summary>
    internal class RabbitReceiver : AbstractReceiver
    {
        #region Static Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        /// <summary>
        /// The _listener registry.
        /// </summary>
        private readonly ListenerRegistry listenerRegistry;

        /// <summary>
        /// The _listener.
        /// </summary>
        private Listener listener;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitReceiver"/>.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="listenerRegistry">
        /// The listener registry.
        /// </param>
        public RabbitReceiver(IReceiverConfiguration configuration, ListenerRegistry listenerRegistry)
            : base(configuration)
        {
            this.listenerRegistry = listenerRegistry;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is started.
        /// </summary>
        public bool IsStarted { get; private set; }

        public override bool IsHealthy
        {
            get
            {
                return (listener != null && !listener.HasFailed);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The can receive.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanReceive(MessageLabel label)
        {
            this.EnsureListenerIsReady();

            return this.listener.Supports(label);
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
        public override void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer)
        {
            Logger.Trace(m => m("Registering consumer of [{0}] for receiver of [{1}].", typeof(T).Name, label));

            this.listener.RegisterConsumer(label, consumer, this.Configuration.Validator);
        }

        /// <summary>
        /// The start.
        /// </summary>
        public override void Start()
        {
            Logger.Trace(m => m("Starting receiver of [{0}].", this.Configuration.Label));

            this.IsStarted = true;

            if (this.listener == null)
            {
                this.BindListener();
            }
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public override void Stop()
        {
            Logger.Trace(m => m("Stopping receiver of [{0}].", this.Configuration.Label));

            this.IsStarted = false;

            this.UnbindListener();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The bind listener.
        /// </summary>
        private void BindListener()
        {
            this.UnbindListener();

            if (!this.IsStarted)
            {
                return;
            }

            Logger.Trace(m => m("Binding listener to receiver of [{0}].", this.Configuration.Label));

            this.listener = this.listenerRegistry.ResolveFor(this.Configuration);
            this.Configuration.ReceiverRegistration(this);

            this.listener.Start();
        }

        /// <summary>
        /// The ensure listener is ready.
        /// </summary>
        private void EnsureListenerIsReady()
        {
            if (this.listener == null && this.IsStarted)
            {
                this.BindListener();
            }
        }

        /// <summary>
        /// The unbind listener.
        /// </summary>
        private void UnbindListener()
        {
            if (this.listener != null)
            {
                Logger.Trace(m => m("Unbinding listener from receiver of [{0}].", this.Configuration.Label));

                this.listener = null;
            }
        }

        #endregion
    }
}
