namespace Contour.Receiving
{
    /// <summary>
    /// The UnhandledDeliveryStrategy interface.
    /// </summary>
    public interface IUnhandledDeliveryStrategy
    {
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="unhandledConsumingContext">
        /// The unhandled consuming context.
        /// </param>
        void Handle(IUnhandledConsumingContext unhandledConsumingContext);
    }
}
