using System;

namespace Contour.Transport.RabbitMQ.Internal
{
    public class ChannelFailedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ChannelFailedEventArgs(Exception ex)
        {
            this.Exception = ex;
        }
    }
}