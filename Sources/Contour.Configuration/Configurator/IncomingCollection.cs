using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The incoming collection.
    /// </summary>
    [ConfigurationCollection(typeof(IncomingElement), AddItemName = "on")]
    internal class IncomingCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The create new element.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new IncomingElement();
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
            return ((IncomingElement)element).Key;
        }
    }
}
