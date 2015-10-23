// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWorker.cs" company="">
//   
// </copyright>
// <summary>
//   The Worker interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Helpers.Scheduler
{
    using System;
    using System.Threading;

    /// <summary>
    /// The Worker interface.
    /// </summary>
    internal interface IWorker : IDisposable
    {
        #region Public Properties

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets the completion handle.
        /// </summary>
        WaitHandle CompletionHandle { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The start.
        /// </summary>
        /// <param name="workAction">
        /// The work action.
        /// </param>
        void Start(Action<CancellationToken> workAction);

        #endregion
    }
}
