namespace Contour.Operators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Content based router.
    /// </summary>
    public class ContentBasedRouter : IMessageOperator
    {
        private readonly Func<IMessage, MessageLabel> routeResolverFunc;

        /// <summary>
        /// Builds operator instance.
        /// </summary>
        /// <param name="routeResolverFunc">Route resolver function.</param>
        public ContentBasedRouter(Func<IMessage, MessageLabel> routeResolverFunc)
        {
            this.routeResolverFunc = routeResolverFunc;
        }

        /// <summary>
        /// Processes incoming message.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <returns>Outcoming messages.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            yield return message.WithLabel(this.routeResolverFunc(message));
        }
    }
}
