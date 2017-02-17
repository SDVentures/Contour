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
        /// Creates a copy of <see cref="RabbitSenderOptions"/>
        /// </summary>
        /// <returns>A copy of this instance</returns>
        public override BusOptions Derive()
        {
            return new RabbitSenderOptions(this);
        }
    }
}