// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractWorker.cs" company="">
//   
// </copyright>
// <summary>
//   The abstract worker.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Helpers.Scheduler
{
    using System;
    using System.Threading;

    /// <summary>
    /// The abstract worker.
    /// </summary>
    internal abstract class AbstractWorker : IWorker
    {
        #region Fields

        /// <summary>
        /// The _completion handle.
        /// </summary>
        private readonly ManualResetEvent _completionHandle = new ManualResetEvent(true);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AbstractWorker"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        protected AbstractWorker(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;

            this.CompletionHandle = new ManualResetEvent(true);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets the completion handle.
        /// </summary>
        public WaitHandle CompletionHandle { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// The start.
        /// </summary>
        /// <param name="workAction">
        /// The work action.
        /// </param>
        public void Start(Action<CancellationToken> workAction)
        {
            this._completionHandle.Reset();
            this.InternalStart(this.WrapAction(workAction));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The internal start.
        /// </summary>
        /// <param name="wrappedWorkAction">
        /// The wrapped work action.
        /// </param>
        protected abstract void InternalStart(Action wrappedWorkAction);

        /// <summary>
        /// The wrap action.
        /// </summary>
        /// <param name="workAction">
        /// The work action.
        /// </param>
        /// <returns>
        /// The <see cref="Action"/>.
        /// </returns>
        protected Action WrapAction(Action<CancellationToken> workAction)
        {
            return () =>
                {
                    try
                    {
                        workAction(this.CancellationToken);
                    }
                    finally
                    {
                        this._completionHandle.Set();
                    }
                };
        }

        #endregion
    }
}
