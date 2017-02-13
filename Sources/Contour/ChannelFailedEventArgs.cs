using System;

namespace Contour
{
    /// <summary>
    /// Encapsulates the event arguments for the channel failed event
    /// </summary>
    public class ChannelFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFailedEventArgs"/> class.
        /// </summary>
        /// <param name="ex">Channel failure reason</param>
        public ChannelFailedEventArgs(Exception ex)
        {
            this.Exception = ex;
        }

        /// <summary>
        /// An error which caused the channel failure
        /// </summary>
        public Exception Exception { get; }

    }
}