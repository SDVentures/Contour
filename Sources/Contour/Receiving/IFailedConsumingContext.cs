// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFailedConsumingContext.cs" company="">
//   
// </copyright>
// <summary>
//   The FailedConsumingContext interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    using System;

    /// <summary>
    /// The FailedConsumingContext interface.
    /// </summary>
    public interface IFailedConsumingContext : IFaultedConsumingContext
    {
        #region Public Properties

        /// <summary>
        /// Gets the exception.
        /// </summary>
        Exception Exception { get; }

        #endregion
    }
}
