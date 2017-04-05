namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The digest section.
    /// </summary>
    public class DigestSection : ConfigurationSection
    {
        /// <summary>
        /// Gets the digests.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DigestCollection Digests
        {
            get
            {
                return (DigestCollection)(base[string.Empty]);
            }
        }
    }
}
