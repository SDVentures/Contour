namespace Contour
{
    /// <summary>
    /// The ChannelProvider interface.
    /// </summary>
    public interface IChannelProvider<out TChannel> where TChannel: IChannel
    {
        TChannel OpenChannel();
    }
}
