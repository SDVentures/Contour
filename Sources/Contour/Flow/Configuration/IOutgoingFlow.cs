namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an outgoing message flow
    /// </summary>
    public interface IOutgoingFlow
    {
        /// <summary>
        /// Sends any flow handling results to the outgoing flow if provided using the same transport as the incoming flow
        /// </summary>
        void Respond(IFlowTarget flow);

        void Forward(string label);
    }
}