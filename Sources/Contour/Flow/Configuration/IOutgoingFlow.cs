namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an outgoing message flow
    /// </summary>
    public interface IOutgoingFlow
    {
        void Respond();
        
        void Forward(string label);
    }
}