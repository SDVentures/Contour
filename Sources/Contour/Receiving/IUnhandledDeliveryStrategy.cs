// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUnhandledDeliveryStrategy.cs" company="">
//   
// </copyright>
// <summary>
//   The UnhandledDeliveryStrategy interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    /// <summary>
    /// The UnhandledDeliveryStrategy interface.
    /// </summary>
    public interface IUnhandledDeliveryStrategy
    {
        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="unhandledConsumingContext">
        /// The unhandled consuming context.
        /// </param>
        void Handle(IUnhandledConsumingContext unhandledConsumingContext);

        #endregion
    }
}
