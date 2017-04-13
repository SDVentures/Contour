using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The endpoints section.
    /// </summary>
    internal class EndpointsSection : ConfigurationSection
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
        public static EndpointsSection Current => _current;

        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EndpointCollection Endpoints => (EndpointCollection)(base[string.Empty]);
    }
}
