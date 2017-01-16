namespace Contour.Flow.Configuration
{
    internal interface IFlowTransport
    {
        IMessageFlow CreateFlow();
    }
}