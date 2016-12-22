namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an incoming message flow. The flow can be configured to push cached messages down the stream.
    /// </summary>
    public interface IMessageFlow : IIncomingFlow
    {
        /// <summary>
        /// Provides a configuration extension point to describe another flow
        /// </summary>
        /// <returns></returns>
        IMessageFlow Also();
    }
}
