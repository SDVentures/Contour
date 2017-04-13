using System;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// The rabbit client.
    /// </summary>
    internal abstract class RabbitClient : IDisposable
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitClient"/>.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        protected RabbitClient(RabbitChannel channel)
        {
            this.Channel = channel;
        }
        /// <summary>
        /// Gets the channel.
        /// </summary>
        protected RabbitChannel Channel { get; private set; }
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
    }
}
