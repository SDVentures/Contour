namespace Contour.Flow.Configuration
{
    internal interface IFlowRegistry
    {
        void RegisterTransport(string name, IFlowTransport transport);

        IFlowEntry Get(string id);
    }
}