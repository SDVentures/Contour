// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaultedConsumingContext.cs" company="">
//   
// </copyright>
// <summary>
//   The faulted consuming context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    /// <summary>
    /// The faulted consuming context.
    /// </summary>
    internal abstract class FaultedConsumingContext : IFaultedConsumingContext
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="FaultedConsumingContext"/>.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        protected FaultedConsumingContext(IDelivery delivery)
        {
            this.Delivery = delivery;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the delivery.
        /// </summary>
        public IDelivery Delivery { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The accept.
        /// </summary>
        public void Accept()
        {
            this.Delivery.Accept();
        }

        /// <summary>
        /// The build fault message.
        /// </summary>
        /// <returns>
        /// The <see cref="FaultMessage"/>.
        /// </returns>
        public abstract FaultMessage BuildFaultMessage();

        /// <summary>
        /// The forward.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Forward<T>(string label, T message) where T : class
        {
            this.Delivery.Forward(label.ToMessageLabel(), message);
        }

        /// <summary>
        /// The reject.
        /// </summary>
        /// <param name="requeue">
        /// The requeue.
        /// </param>
        public void Reject(bool requeue)
        {
            this.Delivery.Reject(requeue);
        }

        #endregion
    }
}
