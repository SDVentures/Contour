using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class ListenerStoppedEventArgs : EventArgs
    {
        public ListenerStoppedEventArgs(IListener listener, OperationStopReason reason)
        {
            this.Listener = listener;
            this.Reason = reason;
        }

        public IListener Listener
        {
            get;
        }

        public OperationStopReason Reason
        {
            get;
        }
    }
}