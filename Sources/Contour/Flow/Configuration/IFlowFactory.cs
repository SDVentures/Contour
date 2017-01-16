namespace Contour.Flow.Configuration
{
    internal interface IFlowFactory
    {
        void RegisterTransport(string name, IFlowTransport transport);
        
        IMessageFlow Create(string transportName);

        IFlowEntry Get(string id);
    }
}