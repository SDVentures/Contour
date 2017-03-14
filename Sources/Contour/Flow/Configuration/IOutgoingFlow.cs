using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Describes an outgoing message flow
    /// </summary>
    public interface IOutgoingFlow<TSource, TOutput>
    {
        IFlowRegistry Registry { set; }
        string Label { set; }

        /// <summary>
        /// Sends any flow handling results to the outgoing flow if provided
        /// </summary>
        IRequestResponseFlow<TSource, TOutput> Respond();

        /// <summary>
        /// Forwards any flow handling results to the flow identified by <paramref name="label"/>
        /// </summary>
        /// <param name="label"></param>
        IRequestResponseFlow<TSource, TOutput> Forward(string label);
    }
}