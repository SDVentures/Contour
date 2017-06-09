using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The endpoint collection.
    /// </summary>
    [ConfigurationCollection(typeof(EndpointElement), AddItemName = "endpoint")]
    public class EndpointCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="EndpointElement"/>.
        /// </returns>
        public new EndpointElement this[string key]
        {
            get
            {
                return BaseGet(key) as EndpointElement;
            }
        }

        /// <summary>
        /// The create new element.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointElement();
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
            return ((EndpointElement)element).Name;
        }
    }
}
