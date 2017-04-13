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
        /// <summary>
        /// The handle confirmation.
        /// </summary>
        /// <param name="confirmed">
        /// The is confirmed.
        /// </param>
        /// <param name="sequenceNumber">
        /// The seq no.
        /// </param>
        /// <param name="multiple">
        /// The is multiple.
        /// </param>
        void HandleConfirmation(bool confirmed, ulong sequenceNumber, bool multiple);

        /// <summary>
        /// The track.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task Track();
    }
}
