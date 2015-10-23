namespace Contour
{
    /// <summary>
    /// The ChannelProvider interface.
    /// </summary>
    public interface IChannelProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The open channel.
        /// </summary>
        /// <returns>
        /// The <see cref="IChannel"/>.
        /// </returns>
        IChannel OpenChannel();

        #endregion
    }
}
