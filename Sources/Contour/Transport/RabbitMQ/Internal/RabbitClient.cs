namespace Contour.Transport.RabbitMQ.Internal
{
    using System;

    /// <summary>
    /// The rabbit client.
    /// </summary>
    internal abstract class RabbitClient : IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="RabbitClient"/>.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        protected RabbitClient(RabbitChannel channel)
        {
            this.Channel = channel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the channel.
        /// </summary>
        protected RabbitChannel Channel { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.Channel != null)
            {
                this.Channel.Dispose();
            }
        }

        #endregion
    }
}
