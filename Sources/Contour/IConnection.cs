using System;
using System.Threading;

namespace Contour
{
    /// <summary>
    /// Describes a bus connection
    /// </summary>
    public interface IConnection : IDisposable
    {
        /// <summary>
        /// Fired when a connection has been opened
        /// </summary>
        event EventHandler Opened;

        /// <summary>
        /// Fired when a connection has been closed
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Fired when a connection has been disposed
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Fired when a connection channel has failed
        /// </summary>
        event EventHandler<ChannelFailedEventArgs> ChannelFailed;

        /// <summary>
        /// Connection identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <param name="token">A token to cancel the operation</param>
        void Open(CancellationToken token);

        /// <summary>
        /// Closes the connection
        /// </summary>
        void Close();

        /// <summary>
        /// Aborts the connection
        /// </summary>
        void Abort();
    }
}