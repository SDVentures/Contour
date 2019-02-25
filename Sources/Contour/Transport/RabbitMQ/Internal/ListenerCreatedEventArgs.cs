using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Holds the data for the event raised when a new listener is created in the <see cref="RabbitReceiver"/>
    /// </summary>
    internal class ListenerCreatedEventArgs : EventArgs
    {
        public ListenerCreatedEventArgs(IListener listener)
        {
            this.Listener = listener;
        }

        /// <summary>
        /// A newly created listener
        /// </summary>
        public IListener Listener
        {
            get;
        }
    }
}