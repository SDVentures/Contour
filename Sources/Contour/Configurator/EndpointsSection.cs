namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The endpoints section.
    /// </summary>
    public class EndpointsSection : ConfigurationSection
    {
        /// <summary>
        /// The _current.
        /// </summary>
        private static readonly EndpointsSection _current = new EndpointsSection();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EndpointsSection"/>.
        /// </summary>
        protected EndpointsSection()
        {
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        public static EndpointsSection Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EndpointCollection Endpoints
        {
            get
            {
                return (EndpointCollection)(base[string.Empty]);
            }
        }
    }
}
