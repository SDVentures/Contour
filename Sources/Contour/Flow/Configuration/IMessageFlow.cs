namespace Contour.Flow.Configuration
{
    public interface IMessageFlow : IIncomingFlow
    {
        IMessageFlow Also();
    }
}
