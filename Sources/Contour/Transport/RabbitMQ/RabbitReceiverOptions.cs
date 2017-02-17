namespace Contour.Transport.RabbitMQ
{
    using Configuration;
    using Helpers;
    using Receiving;

    /// <summary>
    /// The rabbit receiver options.
    /// </summary>
    public class RabbitReceiverOptions : ReceiverOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitReceiverOptions"/> class.
        /// </summary>
        public RabbitReceiverOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitReceiverOptions"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public RabbitReceiverOptions(BusOptions parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Gets or sets the QoS.
        /// </summary>
        public Maybe<QoSParams> QoS { protected get; set; }
        
        /// <summary>
        /// Get the QoS settings.
        /// </summary>
        /// <returns>
        /// QoS settings
        /// </returns>
        public Maybe<QoSParams> GetQoS()
        {
            return this.Pick<RabbitReceiverOptions, QoSParams>((o) => o.QoS);
        }

        /// <summary>
        /// The derive.
        /// </summary>
        /// <returns>
        /// The <see cref="BusOptions"/>.
        /// </returns>
        public override BusOptions Derive()
        {
            return new RabbitReceiverOptions(this);
        }

    }
}
