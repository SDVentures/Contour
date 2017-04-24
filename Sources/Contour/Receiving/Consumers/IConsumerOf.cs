// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConsumerOf.cs" company="">
//   
// </copyright>
// <summary>
//   The ConsumerOf interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving.Consumers
{
    /// <summary>
    /// The ConsumerOf interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IConsumerOf<T> : IConsumer
        where T : class
    {
        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void Handle(IConsumingContext<T> context);

        #endregion
    }
}
