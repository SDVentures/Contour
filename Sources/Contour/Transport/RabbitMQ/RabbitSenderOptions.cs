using Contour.Transport.RabbitMQ.Internal;

namespace Contour.Transport.RabbitMQ
{
    using Configuration;
    using Sending;

    /// <summary>
    /// The rabbit sender options.
    /// </summary>
    internal class RabbitSenderOptions : SenderOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitSenderOptions"/> class.
        /// </summary>
        public RabbitSenderOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitSenderOptions"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public RabbitSenderOptions(BusOptions parent) : base(parent)
        {
        }

        /// <summary>
        /// Gets or sets the maximum time in seconds to wait before making the next attempt to send a message
        /// </summary>
        public int? MaxRetryDelay { protected get; set; }

        /// <summary>
        /// Gets or sets the time in seconds to wait before the retry delays will be reset due to inactivity
        /// </summary>
        public int? InactivityResetDelay { protected get; set; }

        /// <summary>
        /// Treats and returns a connection string as a sequence of RabbitMQ broker URLs
        /// </summary>
        public RabbitConnectionString RabbitConnectionString => new RabbitConnectionString(this.GetConnectionString().Value);

        /// <summary>
        /// Defines an algorithm which specifies the way in which producers will be taken from the list by the sender.
        /// </summary>
        public IProducerSelectorBuilder ProducerSelectorBuilder { protected get; set; }

        /// <summary>
        /// Defines an algorithm which specifies the way in which producers will be taken from the list by the sender.
        /// </summary>
        /// <returns><see cref="IProducerSelector"/></returns>
        public IProducerSelectorBuilder GetProducerSelectorBuilder()
        {
            return this.Pick<RabbitSenderOptions, IProducerSelectorBuilder>((o) => o.ProducerSelectorBuilder);
        }

        /// <summary>
        /// Gets the time in seconds to wait before making the next attempt to send a message
        /// </summary>
        /// <returns>Retry delay in seconds</returns>
        public int? GetMaxRetryDelay()
        {
            return this.Pick<RabbitSenderOptions, int>(o => o.MaxRetryDelay);
        }

        /// <summary>
        /// Gets the time in seconds to wait before the retry delays will be reset due to inactivity
        /// </summary>
        /// <returns>Inactivity reset delay in seconds</returns>
        public int? GetInactivityResetDelay()
        {
            return this.Pick<RabbitSenderOptions, int>(o => o.InactivityResetDelay);
        }

        /// <summary>
        /// Creates a copy of <see cref="RabbitSenderOptions"/>
        /// </summary>
        /// <returns>A copy of this instance</returns>
        public override BusOptions Derive()
        {
            return new RabbitSenderOptions(this);
        }
    }
}