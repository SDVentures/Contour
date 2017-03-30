namespace Contour.Configurator
{
    /// <summary>
    /// Provides a connection string per label.
    /// Overrides a connection string from a configuration section.
    /// </summary>
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// Provides a connection string per label.
        /// </summary>
        /// <param name="messageLabel">An outgoing or incoming label of an endpoint.</param>
        /// <returns>Connection string for the label if presents, or <c>null</c>.</returns>
        string GetConnectionString(MessageLabel messageLabel);
    }
}
