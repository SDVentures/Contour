using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class ChannelFailureEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ChannelFailureEventArgs(Exception ex)
        {
            this.Exception = ex;
        }
    }
}