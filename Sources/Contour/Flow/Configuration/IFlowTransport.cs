namespace Contour.Flow.Configuration
{
    public interface IFlowTransport
    {
        IMessageFlow CreateFlow();
    }
}