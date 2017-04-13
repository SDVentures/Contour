using Contour.Helpers;

namespace Contour.Configuration
{
    /// <summary>
    /// Stores the configuration options related to bus endpoint
    /// </summary>
    public class EndpointOptions : BusOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOptions"/> class.
        /// </summary>
        public EndpointOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOptions"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public EndpointOptions(BusOptions parent) : base(parent)
        {
        }

        /// <summary>
        /// Specifies if a connection should be reused.
        /// </summary>
        public bool? ReuseConnection { protected get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public Maybe<string> ConnectionString { protected get; set; }

        /// <summary>
        /// Gets or sets the number of attempts to take when failing over
        /// </summary>
        public int? FailoverAttempts { protected get; set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        public Maybe<string> GetConnectionString()
        {
            return this.Pick<EndpointOptions, string>(o => o.ConnectionString);
        }

        /// <summary>
        /// Gets the value indicating if a connection can be reused.
        /// </summary>
        /// <returns>
        /// Connection reuse flag
        /// </returns>
        public bool? GetReuseConnection()
        {
            return this.Pick<EndpointOptions, bool>((o) => o.ReuseConnection);
        }

        /// <summary>
        /// Gets the number of attempts to take when failing over
        /// </summary>
        /// <returns>A number of attempts to take</returns>
        public int? GetFailoverAttempts()
        {
            return this.Pick<EndpointOptions, int>(o => o.FailoverAttempts);
        }

        /// <summary>
        /// Creates a copy of <see cref="EndpointOptions"/>
        /// </summary>
        /// <returns>A copy of this instance</returns>
        public override BusOptions Derive()
        {
            return new EndpointOptions(this);
        }
    }
}