namespace Contour.Receiving
{
    using System;

    using Common.Logging;

    /// <summary>
    /// The lambda failed delivery strategy.
    /// </summary>
    public class LambdaFailedDeliveryStrategy : IFailedDeliveryStrategy
    {
        /// <summary>
        /// The _handler action.
        /// </summary>
        private readonly Action<IFailedConsumingContext> _handlerAction;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaFailedDeliveryStrategy"/>.
        /// </summary>
        /// <param name="handlerAction">
        /// The handler action.
        /// </param>
        public LambdaFailedDeliveryStrategy(Action<IFailedConsumingContext> handlerAction)
        {
            this._handlerAction = handlerAction;
        }
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="failedConsumingContext">
        /// The failed consuming context.
        /// </param>
        public void Handle(IFailedConsumingContext failedConsumingContext)
        {
            try
            {
                this._handlerAction(failedConsumingContext);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger<LambdaFailedDeliveryStrategy>().
                    ErrorFormat("Unable to handle failed message [{0}].", ex, failedConsumingContext.Delivery.Label);
            }
        }
    }
}
