namespace Contour.Receiving
{
    using System;

    /// <summary>
    /// The FailedConsumingContext interface.
    /// </summary>
    public interface IFailedConsumingContext : IFaultedConsumingContext
    {
        /// <summary>
        /// Gets the exception.
        /// </summary>
        Exception Exception { get; }
    }
}
