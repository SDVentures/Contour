namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The confirmation handler.
    /// </summary>
    /// <param name="isConfirmed">
    /// The is confirmed.
    /// </param>
    /// <param name="seqNo">
    /// The seq no.
    /// </param>
    /// <param name="isMultiple">
    /// The is multiple.
    /// </param>
    internal delegate void ConfirmationHandler(bool isConfirmed, ulong seqNo, bool isMultiple);

    /// <summary>
    /// The PublishConfirmationTracker interface.
    /// </summary>
    internal interface IPublishConfirmationTracker : IDisposable
    {
        #region Public Methods and Operators

        /// <summary>
        /// The handle confirmation.
        /// </summary>
        /// <param name="isConfirmed">
        /// The is confirmed.
        /// </param>
        /// <param name="seqNo">
        /// The seq no.
        /// </param>
        /// <param name="isMultiple">
        /// The is multiple.
        /// </param>
        void HandleConfirmation(bool isConfirmed, ulong seqNo, bool isMultiple);

        /// <summary>
        /// The track.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task Track();

        #endregion
    }
}
