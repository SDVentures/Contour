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
