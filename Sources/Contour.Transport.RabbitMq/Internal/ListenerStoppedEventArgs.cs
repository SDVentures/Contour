using System;

namespace Contour.Transport.RabbitMq.Internal
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