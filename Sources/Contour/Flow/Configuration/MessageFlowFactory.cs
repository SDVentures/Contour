namespace Contour.Flow.Configuration
{
    public class MessageFlowFactory : IMessageFlowFactory
    {
        public static IMessageFlow Build()
        {
            return new MessageFlow();
        }

        IMessageFlow IMessageFlowFactory.Build()
        {
            return Build();
        }
    }
}