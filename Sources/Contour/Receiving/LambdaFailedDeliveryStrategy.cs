// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LambdaFailedDeliveryStrategy.cs" company="">
//   
// </copyright>
// <summary>
//   The lambda failed delivery strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    using System;

    using Common.Logging;

    /// <summary>
    /// The lambda failed delivery strategy.
    /// </summary>
    public class LambdaFailedDeliveryStrategy : IFailedDeliveryStrategy
    {
        #region Fields

        /// <summary>
        /// The _handler action.
        /// </summary>
        private readonly Action<IFailedConsumingContext> _handlerAction;

        #endregion

        #region Constructors and Destructors

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

        #endregion

        #region Public Methods and Operators

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
                LogManager.GetCurrentClassLogger().
                    ErrorFormat("Unable to handle failed message [{0}].", ex, failedConsumingContext.Delivery.Label);
            }
        }

        #endregion
    }
}
