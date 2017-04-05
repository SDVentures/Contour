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
        /// <summary>
        /// The _task.
        /// </summary>
        private Task _task;
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
    }
}
