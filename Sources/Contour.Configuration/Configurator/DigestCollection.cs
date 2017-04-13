using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The digest collection.
    /// </summary>
    [ConfigurationCollection(typeof(DigestElement), AddItemName = "digest")]
    public class DigestCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The create new element.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DigestElement();
        }

        /// <summary>
        /// The get element key.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DigestElement)element).Type;
        }
    }
}
