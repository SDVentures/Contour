// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultBusLifecycleHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The default bus lifecycle handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour
{
    using System;

    /// <summary>
    /// The default bus lifecycle handler.
    /// </summary>
    public class DefaultBusLifecycleHandler : IBusLifecycleHandler
    {
        #region Public Methods and Operators

        /// <summary>
        /// The on started.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public virtual void OnStarted(IBus bus, EventArgs args)
        {
            // no-op
        }

        /// <summary>
        /// The on starting.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public virtual void OnStarting(IBus bus, EventArgs args)
        {
            // no-op
        }

        /// <summary>
        /// The on stopped.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public virtual void OnStopped(IBus bus, EventArgs args)
        {
            // no-op
        }

        /// <summary>
        /// The on stopping.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public virtual void OnStopping(IBus bus, EventArgs args)
        {
            // no-op
        }

        #endregion
    }
}
