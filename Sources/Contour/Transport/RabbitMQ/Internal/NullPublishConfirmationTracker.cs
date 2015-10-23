namespace Contour.Transport.RabbitMQ.Internal
{
    using System.Threading.Tasks;

    /// <summary>
    /// The null publish confirmation tracker.
    /// </summary>
    internal class NullPublishConfirmationTracker : IPublishConfirmationTracker
    {
        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

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
        public void HandleConfirmation(bool isConfirmed, ulong seqNo, bool isMultiple)
        {
        }

        /// <summary>
        /// The track.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Track()
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(null);
            return completionSource.Task;
        }

        #endregion
    }
}
