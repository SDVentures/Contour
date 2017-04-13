using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The digest section.
    /// </summary>
    public class DigestSection : ConfigurationSection
    {
        /// <summary>
        /// Gets the digests.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DigestCollection Digests => (DigestCollection)(base[string.Empty]);
    }
}
