using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Holds the data for the event raised when a new listener is registered in the <see cref="RabbitReceiver"/>
    /// </summary>
    internal class ListenerRegisteredEventArgs : EventArgs
    {
        public ListenerRegisteredEventArgs(IListener listener)
        {
            this.Listener = listener;
        }

        /// <summary>
        /// A newly registered listener
        /// </summary>
        public IListener Listener
        {
            get;
        }
    }
}