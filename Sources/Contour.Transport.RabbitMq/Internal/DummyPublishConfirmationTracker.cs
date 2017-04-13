using System.Threading.Tasks;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// A dummy confirmation tracker which should not receive confirmations from the broker and will do nothing if one is received
    /// </summary>
    internal class DummyPublishConfirmationTracker : IPublishConfirmationTracker
    {
        public void Dispose()
        {
        }

        /// <summary>
        /// A dummy handler which can be registered to receive publish confirmations from the broker. No actions on messages are performed.
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
        public void HandleConfirmation(bool confirmed, ulong sequenceNumber, bool multiple)
        {
        }

        /// <summary>
        /// Registers a new message publishing confirmation
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
    }
}
