﻿namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The validator collection.
    /// </summary>
    [ConfigurationCollection(typeof(ValidatorElement), AddItemName = "add")]
    internal class ValidatorCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The create new element.
        /// </summary>
        /// <returns>
        /// The <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ValidatorElement();
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
            return ((ValidatorElement)element).Name;
        }
    }
}
