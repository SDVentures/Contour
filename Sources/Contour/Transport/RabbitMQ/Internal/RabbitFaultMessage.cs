namespace Contour.Transport.RabbitMQ.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Contour.Helpers;

    /// <summary>
    /// The rabbit fault message.
    /// </summary>
    internal class RabbitFaultMessage : FaultMessage
    {
        #region Static Fields

        /// <summary>
        /// The text based content types.
        /// </summary>
        private static readonly ISet<string> TextBasedContentTypes = new HashSet<string> { "text/plain", "text/html", "text/xml", "application/json", "application/xml" };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitFaultMessage"/>.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public RabbitFaultMessage(RabbitDelivery delivery, Exception exception)
            : base(new Message(delivery.Label, delivery.Headers, TextBasedContentTypes.Contains(delivery.Args.BasicProperties.ContentType) ? (object)Encoding.UTF8.GetString(delivery.Args.Body) : delivery.Args.Body), DateTimeEx.FromUnixTimestamp(delivery.Args.BasicProperties.Timestamp.UnixTime), delivery.Args.BasicProperties.ContentType, exception)
        {
            this.Route = new RabbitRoute(delivery.Args.Exchange, delivery.Args.RoutingKey);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitFaultMessage"/>.
        /// </summary>
        /// <param name="delivery">
        /// The delivery.
        /// </param>
        public RabbitFaultMessage(RabbitDelivery delivery)
            : this(delivery, null)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the route.
        /// </summary>
        public RabbitRoute Route { get; private set; }

        #endregion
    }
}
