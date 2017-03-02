namespace Contour
{
    /// <summary>
    /// Creates channels of <typeparamref name="TChannel"/> type using the underlying connection.
    /// </summary>
    /// <typeparam name="TChannel">The type of channel to be created</typeparam>
    public interface IChannelProvider<out TChannel>
        where TChannel : IChannel
    {
        /// <summary>
        /// Opens a new channel in the underlying connection
        /// </summary>
        /// <returns>An open channel</returns>
        TChannel OpenChannel();
    }
}
