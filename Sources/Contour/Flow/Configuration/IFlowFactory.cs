namespace Contour.Flow.Configuration
{
    public interface IFlowFactory
    {
        IMessageFlow Create(string transportName);
    }
}