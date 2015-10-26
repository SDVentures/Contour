// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskWorker.cs" company="">
//   
// </copyright>
// <summary>
//   The task worker.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Helpers.Scheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The task worker.
    /// </summary>
    internal class TaskWorker : AbstractWorker
    {
        #region Fields

        /// <summary>
        /// The _task.
        /// </summary>
        private Task _task;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TaskWorker"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public TaskWorker(CancellationToken cancellationToken)
            : base(cancellationToken)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public override void Dispose()
        {
            if (this._task != null)
            {
                this._task.Wait();
                this._task = null;
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
            this._task = Task.Factory.StartNew(workAction, this.CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion
    }
}
