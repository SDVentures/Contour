// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConsumerOf.cs" company="">
//   
// </copyright>
// <summary>
//   The ConsumerOf interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Contour.Receiving.Consumers
{
    /// <summary>
    /// The ConsumerOf interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    [Obsolete]
    public interface IConsumerOf<T> : IConsumer<T>
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
