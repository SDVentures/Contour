namespace Contour
{
    /// <summary>
    /// Creates connections on demand
    /// </summary>
    /// <typeparam name="TConnection">The type of connection to be created</typeparam>
    public interface IConnectionProvider<out TConnection>
        where TConnection : IConnection
    {
        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="connectionString">
        /// A connection string containing the destination address
        /// </param>
        /// <returns>
        /// A new connection
        /// </returns>
        TConnection Create(string connectionString);
    }
}