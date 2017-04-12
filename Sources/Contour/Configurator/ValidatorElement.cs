﻿namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The validator element.
    /// </summary>
    internal class ValidatorElement : ConfigurationElement
    {
        /// <summary>
        /// Gets a value indicating whether group.
        /// </summary>
        [ConfigurationProperty("group", IsRequired = false)]
        public bool Group
        {
            get
            {
                return (bool)(base["group"]);
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)(base["name"]);
            }
        }
    }
}
