// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISubscriptionEndpoint.cs" company="">
//   
// </copyright>
// <summary>
//   The SubscriptionEndpoint interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    using Contour.Sending;

    /// <summary>
    /// The SubscriptionEndpoint interface.
    /// </summary>
    public interface ISubscriptionEndpoint
    {
        /// <summary>
        /// Gets the callback route resolver.
        /// </summary>
        IRouteResolver CallbackRouteResolver { get; }

        /// <summary>
        /// Gets the listening source.
        /// </summary>
        IListeningSource ListeningSource { get; }
    }
}
