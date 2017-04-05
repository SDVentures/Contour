namespace Contour.Receiving
{
    using Contour.Sending;

    /// <summary>
    /// The subscription endpoint.
    /// </summary>
    public class SubscriptionEndpoint : ISubscriptionEndpoint
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SubscriptionEndpoint"/>.
        /// </summary>
        /// <param name="listeningSource">
        /// The listening source.
        /// </param>
        /// <param name="callbackRouteResolver">
        /// The callback route resolver.
        /// </param>
        public SubscriptionEndpoint(IListeningSource listeningSource, IRouteResolver callbackRouteResolver = null)
        {
            this.ListeningSource = listeningSource;
            this.CallbackRouteResolver = callbackRouteResolver;
        }
        /// <summary>
        /// Gets the callback route resolver.
        /// </summary>
        public IRouteResolver CallbackRouteResolver { get; private set; }

        /// <summary>
        /// Gets the listening source.
        /// </summary>
        public IListeningSource ListeningSource { get; private set; }
    }
}
