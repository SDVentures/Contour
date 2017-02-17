namespace Contour.Configuration
{
    using Helpers;

    /// <summary>
    /// Endpoint options
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

        public Maybe<bool> ReuseConnection { protected get; set; }

        public Maybe<string> ConnectionString { protected get; set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns>
        /// The <see cref="Maybe{T}"/>.
        /// </returns>
        public Maybe<string> GetConnectionString()
        {
            return this.Pick<EndpointOptions, string>((o) => o.ConnectionString);
        }

        public Maybe<bool> GetReuseConnection()
        {
            return this.Pick<EndpointOptions, bool>((o) => o.ReuseConnection);
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