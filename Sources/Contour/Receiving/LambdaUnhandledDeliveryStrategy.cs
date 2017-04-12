﻿namespace Contour.Receiving
{
    using System;

    using Common.Logging;

    /// <summary>
    /// The lambda unhandled delivery strategy.
    /// </summary>
    public class LambdaUnhandledDeliveryStrategy : IUnhandledDeliveryStrategy
    {
        /// <summary>
        /// The _handler action.
        /// </summary>
        private readonly Action<IUnhandledConsumingContext> _handlerAction;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaUnhandledDeliveryStrategy"/>.
        /// </summary>
        /// <param name="handlerAction">
        /// The handler action.
        /// </param>
        public LambdaUnhandledDeliveryStrategy(Action<IUnhandledConsumingContext> handlerAction)
        {
            this._handlerAction = handlerAction;
        }
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="unhandledConsumingContext">
        /// The unhandled consuming context.
        /// </param>
        public void Handle(IUnhandledConsumingContext unhandledConsumingContext)
        {
            try
            {
                this._handlerAction(unhandledConsumingContext);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger<LambdaUnhandledDeliveryStrategy>().
                    ErrorFormat("Unable to handle failed message [{0}].", ex, unhandledConsumingContext.Delivery.Label);
            }
        }
    }
}
