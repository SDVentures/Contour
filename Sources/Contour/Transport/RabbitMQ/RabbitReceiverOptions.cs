namespace Contour.Transport.RabbitMQ
{
    using Contour.Configuration;
    using Contour.Helpers;
    using Contour.Receiving;

    /// <summary>
    /// The rabbit receiver options.
    /// </summary>
    public class RabbitReceiverOptions : ReceiverOptions
    {
        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RabbitReceiverOptions"/>.
        /// </summary>
        public RabbitReceiverOptions()
        {
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RabbitReceiverOptions"/>.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public RabbitReceiverOptions(BusOptions parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Gets or sets the qo s.
        /// </summary>
        public Maybe<QoSParams> QoS { protected get; set; }

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

        /// <summary>
        /// The get qo s.
        /// </summary>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        public Maybe<QoSParams> GetQoS()
        {
            return this.Pick(o => ((RabbitReceiverOptions)o).QoS);
        }
    }
}
