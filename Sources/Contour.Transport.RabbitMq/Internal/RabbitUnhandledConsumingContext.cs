using Contour.Receiving;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// The rabbit unhandled consuming context.
    /// </summary>
    internal class RabbitUnhandledConsumingContext : FaultedConsumingContext, IUnhandledConsumingContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitUnhandledConsumingContext"/>.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        public RabbitUnhandledConsumingContext(RabbitDelivery delivery)
            : base(delivery)
        {
        }
        /// <summary>
        /// The build fault message.
        /// </summary>
        /// <returns>
        /// The <see cref="FaultMessage"/>.
        /// </returns>
        public override FaultMessage BuildFaultMessage()
        {
            return new RabbitFaultMessage((RabbitDelivery)this.Delivery);
        }
    }
}
