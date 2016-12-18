namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Builds an empty message flow
    /// </summary>
    public class FlowFactory
    {
        public static IMessageFlow Build()
        {
            return new MessageFlow();
        }
    }
}