namespace Contour.Flow.Configuration
{
    public interface IIncomingFlow: IFlow
    {
        IActingFlow<TOutput> On<TOutput>(string label, int capacity = 1);
    }
}