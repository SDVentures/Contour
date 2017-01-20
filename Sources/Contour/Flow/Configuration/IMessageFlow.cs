namespace Contour.Flow.Configuration
{
    public interface IMessageFlow : IIncomingFlow, IFlowEntry, IFlowTarget, IFlowRegistryItem
    {
        IFlowLabelProvider LabelProvider { set; }
    }
}