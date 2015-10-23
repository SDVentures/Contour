namespace Contour
{
    using System;

    /// <summary>
    /// The BusLifecycleHandler interface.
    /// </summary>
    public interface IBusLifecycleHandler
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
        void OnStarted(IBus bus, EventArgs args);

        /// <summary>
        /// The on starting.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void OnStarting(IBus bus, EventArgs args);

        /// <summary>
        /// The on stopped.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void OnStopped(IBus bus, EventArgs args);

        /// <summary>
        /// The on stopping.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void OnStopping(IBus bus, EventArgs args);

        #endregion
    }
}
