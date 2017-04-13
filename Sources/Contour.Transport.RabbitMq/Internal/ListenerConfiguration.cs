using Contour.Receiving;
using Contour.Sending;
using Contour.Validation;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// The listener configuration.
    /// </summary>
    internal class ListenerConfiguration
    {
        /// <summary>
        /// Gets the callback route resolver.
        /// </summary>
        public IRouteResolver CallbackRouteResolver
        {
            get
            {
                return this.SubscriptionEndpoint.CallbackRouteResolver;
            }
        }

        /// <summary>
        /// Gets or sets the failed delivery strategy.
        /// </summary>
        public IFailedDeliveryStrategy FailedDeliveryStrategy { get; set; }

        /// <summary>
        /// Gets the listening source.
        /// </summary>
        public IListeningSource ListeningSource
        {
            get
            {
                return this.SubscriptionEndpoint.ListeningSource;
            }
        }

        /// <summary>
        /// Gets or sets the parallelism level.
        /// </summary>
        public uint ParallelismLevel { get; set; }

        /// <summary>
        /// Gets or sets the qos.
        /// </summary>
        public QoSParams Qos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requires accept.
        /// </summary>
        public bool RequiresAccept { get; set; }

        /// <summary>
        /// Gets or sets the subscription endpoint.
        /// </summary>
        public ISubscriptionEndpoint SubscriptionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the unhandled delivery strategy.
        /// </summary>
        public IUnhandledDeliveryStrategy UnhandledDeliveryStrategy { get; set; }

        /// <summary>
        /// Gets or sets the validator registry.
        /// </summary>
        public MessageValidatorRegistry ValidatorRegistry { get; set; }
    }
}
