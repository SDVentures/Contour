namespace Contour.Flow.Configuration
{
    internal interface IMessageFlow: IIncomingFlow, IFlowEntry
    {
         string Id { get; }
    }
}