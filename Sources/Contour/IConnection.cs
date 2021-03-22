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
        /// Connection identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Connection string
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Ключ строки подключения, который может идентифицировать подключение по параметрам из connString, для RMQ IP + Port + vhost
        /// </summary>
        string ConnectionKey { get; }

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