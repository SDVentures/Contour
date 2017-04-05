// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFaultedConsumingContext.cs" company="">
//   
// </copyright>
// <summary>
//   The FaultedConsumingContext interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    /// <summary>
    /// The FaultedConsumingContext interface.
    /// </summary>
    public interface IFaultedConsumingContext
    {
        /// <summary>
        /// Gets the delivery.
        /// </summary>
        IDelivery Delivery { get; }
        /// <summary>
        /// The accept.
        /// </summary>
        void Accept();

        /// <summary>
        /// The build fault message.
        /// </summary>
        /// <returns>
        /// The <see cref="FaultMessage"/>.
        /// </returns>
        FaultMessage BuildFaultMessage();

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
        void Forward<T>(string label, T message) where T : class;

        /// <summary>
        /// The reject.
        /// </summary>
        /// <param name="requeue">
        /// The requeue.
        /// </param>
        void Reject(bool requeue);
    }
}
