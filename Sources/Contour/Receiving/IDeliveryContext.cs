// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeliveryContext.cs" company="">
//   
// </copyright>
// <summary>
//   The DeliveryContext interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    /// <summary>
    /// The DeliveryContext interface.
    /// </summary>
    public interface IDeliveryContext
    {
        /// <summary>
        /// Gets the delivery.
        /// </summary>
        IDelivery Delivery { get; }
    }
}
