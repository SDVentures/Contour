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
        [Obsolete]
        void Handle(IConsumingContext<T> context);

        #endregion
    }


    //todo отделить от IConsumerOf<T>
    public interface IAsyncConsumerOf<T> : IConsumerOf<T>
      where T : class
    {
        #region Public Methods and Operators

        Task HandleAsync(IConsumingContext<T> context);

        #endregion
    }
}
