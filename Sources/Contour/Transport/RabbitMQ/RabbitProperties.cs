// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RabbitProperties.cs" company="">
//   
// </copyright>
// <summary>
//   The rabbit properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// The rabbit properties.
    /// </summary>
    public static class RabbitProperties
    {
        /// <summary>
        /// The default qo s.
        /// </summary>
        public const string DefaultQoS = "rabbit-default-qos";

        /// <summary>
        /// The qo s.
        /// </summary>
        public const string QoS = "rabbit-qos";

        /// <summary>
        /// The subscription builder.
        /// </summary>
        public const string SubscriptionBuilder = "rabbit-subscription-builder";
    }
}
