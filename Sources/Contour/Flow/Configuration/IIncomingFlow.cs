namespace Contour.Flow.Configuration
{
    public interface IIncomingFlow: IFlow
    {
        IActionableFlow<TOutput> On<TOutput>(string label, int capacity = 1);
    }
}