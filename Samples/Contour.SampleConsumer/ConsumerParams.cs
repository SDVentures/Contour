namespace Contour.SampleConsumer
{
    using System.Configuration;

    /// <summary>
    /// The consumer params.
    /// </summary>
    internal class ConsumerParams
    {
        #region Constants

        /// <summary>
        /// The one way.
        /// </summary>
        public const string OneWay = "one-way";

        /// <summary>
        /// The request.
        /// </summary>
        public const string Request = "request";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString { get; internal set; }

        /// <summary>
        /// Gets the consuming delay.
        /// </summary>
        public uint ConsumingDelay { get; internal set; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public string Endpoint { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether misbehave.
        /// </summary>
        public bool Misbehave { get; internal set; }

        /// <summary>
        /// Gets the parallelism level.
        /// </summary>
        public uint ParallelismLevel { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether require accept.
        /// </summary>
        public bool RequireAccept { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public string Type { get; internal set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read from app settings.
        /// </summary>
        /// <returns>
        /// The <see cref="ConsumerParams"/>.
        /// </returns>
        public static ConsumerParams ReadFromAppSettings()
        {
            return new ConsumerParams
                       {
                           ConnectionString = ConfigurationManager.ConnectionStrings["service-bus"].ConnectionString, 
                           Endpoint = ConfigurationManager.AppSettings["endpoint"], 
                           Type = ConfigurationManager.AppSettings["type"], 
                           ParallelismLevel = uint.Parse(ConfigurationManager.AppSettings["parallelism-level"]), 
                           RequireAccept = bool.Parse(ConfigurationManager.AppSettings["require-accept"]), 
                           Misbehave = bool.Parse(ConfigurationManager.AppSettings["misbehave"]), 
                           ConsumingDelay = uint.Parse(ConfigurationManager.AppSettings["consuming-delay"])
                       };
        }

        #endregion
    }
}
