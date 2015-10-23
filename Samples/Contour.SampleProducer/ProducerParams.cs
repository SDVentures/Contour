namespace Contour.SampleProducer
{
    using System;
    using System.Configuration;

    /// <summary>
    /// The producer params.
    /// </summary>
    internal class ProducerParams
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
        /// Gets the delay.
        /// </summary>
        public int Delay { get; internal set; }

        /// <summary>
        /// Gets the emitting threads.
        /// </summary>
        public int EmittingThreads { get; internal set; }

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
        /// Gets a value indicating whether require confirmation.
        /// </summary>
        public bool RequireConfirmation { get; internal set; }

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
        /// The <see cref="ProducerParams"/>.
        /// </returns>
        public static ProducerParams ReadFromAppSettings()
        {
            return new ProducerParams
                       {
                           ConnectionString = ConfigurationManager.ConnectionStrings["service-bus"].ConnectionString, 
                           Endpoint = ConfigurationManager.AppSettings["endpoint"], 
                           Type = ConfigurationManager.AppSettings["type"], 
                           Delay = int.Parse(ConfigurationManager.AppSettings["delay"]), 
                           EmittingThreads = int.Parse(ConfigurationManager.AppSettings["emitting-threads"]), 
                           ParallelismLevel = uint.Parse(ConfigurationManager.AppSettings["parallelism-level"]), 
                           RequireConfirmation = bool.Parse(ConfigurationManager.AppSettings["require-confirmation"]), 
                           Misbehave = bool.Parse(ConfigurationManager.AppSettings["misbehave"])
                       };
        }

        #endregion
    }
}
