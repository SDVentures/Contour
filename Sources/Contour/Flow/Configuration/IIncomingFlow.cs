namespace Contour.Flow.Configuration
{
    public interface IIncomingFlow
    {
        IActionableFlow On<TInput>(string label, int capacity = 1);
    }
}