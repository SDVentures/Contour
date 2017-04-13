namespace Contour.Transport.RabbitMQ.Internal
{
    using System;

    using Contour.Receiving;

    /// <summary>
    /// The rabbit failed consuming context.
    /// </summary>
    internal class RabbitFailedConsumingContext : FaultedConsumingContext, IFailedConsumingContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitFailedConsumingContext"/>.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public RabbitFailedConsumingContext(RabbitDelivery delivery, Exception exception)
            : base(delivery)
        {
            this.Exception = exception;
        }
        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// The build fault message.
        /// </summary>
        /// <returns>
        /// The <see cref="FaultMessage"/>.
        /// </returns>
        public override FaultMessage BuildFaultMessage()
        {
            return new RabbitFaultMessage((RabbitDelivery)this.Delivery, this.Exception);
        }
    }
}
