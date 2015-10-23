namespace Contour.Transport.RabbitMQ.Internal
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Contour.Helpers;
    using Contour.Sending;

    /// <summary>
    /// The default publish confirmation tracker.
    /// </summary>
    internal class DefaultPublishConfirmationTracker : IPublishConfirmationTracker
    {
        #region Fields

        /// <summary>
        /// The _channel.
        /// </summary>
        private readonly RabbitChannel channel;

        /// <summary>
        /// The _pending.
        /// </summary>
        private readonly IDictionary<ulong, TaskCompletionSource<object>> pending = new ConcurrentDictionary<ulong, TaskCompletionSource<object>>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="DefaultPublishConfirmationTracker"/>.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public DefaultPublishConfirmationTracker(RabbitChannel channel)
        {
            this.channel = channel;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Reset();
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
            if (isMultiple)
            {
                this.pending.Keys.Where(r => r <= seqNo).
                    ToArray().
                    ForEach(k => this.ProcessConfirmation(k, isConfirmed));
            }
            else
            {
                this.ProcessConfirmation(seqNo, isConfirmed);
            }
        }

        /// <summary>
        /// The reset.
        /// </summary>
        public void Reset()
        {
            if (this.pending == null)
            {
                return;
            }

            this.pending.Values.ForEach(v => v.TrySetException(new MessageRejectedException()));
            this.pending.Clear();
        }

        /// <summary>
        /// The track.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Track()
        {
            // TODO: channel publishing is not threadsafe!
            var completionSource = new TaskCompletionSource<object>();
            this.pending.Add(this.channel.GetNextSeqNo(), completionSource);

            return completionSource.Task;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The process confirmation.
        /// </summary>
        /// <param name="seqNo">
        /// The seq no.
        /// </param>
        /// <param name="isConfirmed">
        /// The is confirmed.
        /// </param>
        private void ProcessConfirmation(ulong seqNo, bool isConfirmed)
        {
            TaskCompletionSource<object> completionSource;
            if (this.pending.TryGetValue(seqNo, out completionSource))
            {
                if (this.pending.Remove(seqNo))
                {
                    if (isConfirmed)
                    {
                        completionSource.TrySetResult(null);
                    }
                    else
                    {
                        completionSource.TrySetException(new MessageRejectedException());
                    }
                }
            }
        }

        #endregion
    }
}
