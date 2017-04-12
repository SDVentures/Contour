﻿namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The message element.
    /// </summary>
    internal class MessageElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)base["key"];
            }
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        [ConfigurationProperty("label", IsRequired = true)]
        public string Label
        {
            get
            {
                return (string)base["label"];
            }
        }
    }
}
