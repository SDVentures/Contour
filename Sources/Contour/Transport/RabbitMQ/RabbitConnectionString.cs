namespace Contour.Transport.RabbitMQ
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Enables RabbitMQ connection string operations
    /// </summary>
    public class RabbitConnectionString : IEnumerable<string>
    {
        private const string Separator = ",";
        private readonly IList<string> urls;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitConnectionString"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Raises an error if connection string is null or empty
        /// </exception>
        public RabbitConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.urls = connectionString.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

            if (!this.urls.Any())
            {
                throw new ArgumentException($"The provided connection string does not contain any URLs");
            }
        }

        /// <summary>
        /// Gets the connection strings enumerator
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            return this.urls.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
