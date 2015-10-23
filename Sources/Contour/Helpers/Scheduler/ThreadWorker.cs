// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadWorker.cs" company="">
//   
// </copyright>
// <summary>
//   The thread worker.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Helpers.Scheduler
{
    using System;
    using System.Threading;

    /// <summary>
    /// The thread worker.
    /// </summary>
    internal class ThreadWorker : AbstractWorker
    {
        #region Fields

        /// <summary>
        /// The _thread.
        /// </summary>
        private Thread _thread;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="ThreadWorker"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public ThreadWorker(CancellationToken cancellationToken)
            : base(cancellationToken)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The start new.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="IWorker"/>.
        /// </returns>
        public static IWorker StartNew(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            var worker = new ThreadWorker(cancellationToken);
            worker.Start(action);
            return worker;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public override void Dispose()
        {
            if (this._thread != null)
            {
                this._thread.Join(); // TODO: timeout, then .Intercept() ?
                this._thread = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The internal start.
        /// </summary>
        /// <param name="workAction">
        /// The work action.
        /// </param>
        protected override void InternalStart(Action workAction)
        {
            var threadAction = new ThreadStart(workAction);
            this._thread = new Thread(threadAction) { IsBackground = true };
            this._thread.Start();
        }

        #endregion
    }
}
