namespace Contour.Flow.Configuration
{
    public interface IMessageFlow: IIncomingFlow, IFlowEntry
    {
         string Id { get; }
    }
}