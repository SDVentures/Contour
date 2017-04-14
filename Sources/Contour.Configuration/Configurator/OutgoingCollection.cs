﻿using System.Configuration;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    /// The outgoing collection.
    /// </summary>
    [ConfigurationCollection(typeof(OutgoingElement), AddItemName = "route")]
    public class OutgoingCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The create new element.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new OutgoingElement();
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
            return ((OutgoingElement)element).Key;
        }
    }
}
