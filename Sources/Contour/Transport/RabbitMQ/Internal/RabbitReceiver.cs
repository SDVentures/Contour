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
        
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields
        
        private readonly RabbitListener rabbitListener;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitReceiver"/>.
        /// </summary>
        /// <param name="rabbitBus"></param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        public RabbitReceiver(RabbitBus rabbitBus, IReceiverConfiguration configuration) : base(configuration)
        {
            Logger.Trace(m => m("Binding listener to receiver of [{0}].", this.Configuration.Label));

            var listenerConfig = new RabbitListenerConfiguration(configuration);
            this.rabbitListener = new RabbitListener(rabbitBus, listenerConfig);
            this.Configuration.ReceiverRegistration(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether receiver is started.
        /// </summary>
        public bool IsStarted { get; private set; }

        public override bool IsHealthy
        {
            get
            {
                return (rabbitListener != null && !rabbitListener.HasFailed);
            }
        }

        #endregion

        #region Public Methods and Operators

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
            this.rabbitListener.RegisterConsumer(label, consumer, this.Configuration.Validator);
        }

        /// <summary>
        /// The start.
        /// </summary>
        public override void Start()
        {
            if (IsStarted)
            {
                Logger.Trace($"Unable to start, receiver of [{this.Configuration.Label}] is already running.");
                return;
            }

            Logger.Trace(m => m("Starting receiver of [{0}].", this.Configuration.Label));
            
            this.rabbitListener.Start();
            this.IsStarted = true;
            Logger.Trace(m => m("Receiver of [{0}] has started successfully.", this.Configuration.Label));
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public override void Stop()
        {
            if (!IsStarted)
            {
                Logger.Trace($"Unable to stop, receiver of [{this.Configuration.Label}] is not running.");
                return;
            }

            Logger.Trace(m => m("Stopping receiver of [{0}].", this.Configuration.Label));
            
            rabbitListener.Stop();
            this.IsStarted = false;
            Logger.Trace(m => m("Receiver of [{0}] has stopped successfully.", this.Configuration.Label));
        }

        #endregion
    }
}
