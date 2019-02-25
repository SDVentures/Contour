using System.Linq;

using Contour.Configurator.Configuration;

namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The digest section.
    /// </summary>
    public class DigestSection : ConfigurationSection, IDigestSection
    {
        #region Public Properties

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

        IDigest[] IDigestSection.Digests => this.Digests.Cast<IDigest>().ToArray();

        #endregion
    }
}
