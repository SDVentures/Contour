namespace Contour.Configurator
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// The message element.
    /// </summary>
    public class MessageElement : ConfigurationElement
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

        /// <summary>
        /// Configuration extension properties
        /// </summary>
        public IDictionary<string, string> ExtensionProperties { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Configuration extension elements
        /// </summary>
        public IDictionary<string, XNode> ExtensionConfigurations { get; } = new Dictionary<string, XNode>();

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization
        /// </summary>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false
        /// </returns>
        /// <param name="name">The name of the unrecognized attribute</param>
        /// <param name="value">The value of the unrecognized attribute</param>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this.ExtensionProperties[name] = value;
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether an unknown element is encountered during deserialization
        /// </summary>
        /// <returns>
        /// true when an unknown element is encountered while deserializing; otherwise, false
        /// </returns>
        /// <param name="elementName">The name of the unknown subelement</param>
        /// <param name="reader">The XmlReader being used for deserialization</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by elementName is locked.
        /// - or -
        /// One or more of the element's attributes is locked.
        /// - or -
        /// elementName is unrecognized, or the element has an unrecognized attribute.
        /// - or -
        /// The element has a Boolean attribute with an invalid value.
        /// - or -
        /// An attempt was made to deserialize a property more than once.
        /// - or -
        /// An attempt was made to deserialize a property that is not a valid member of the element.
        /// - or -
        /// The element cannot contain a CDATA or text element.</exception>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            try
            {
                this.ExtensionConfigurations[elementName] = XNode.ReadFrom(reader);
                return true;
            }
            catch (Exception e)
            {
                throw new ConfigurationErrorsException($"Error reading from '{elementName}' configuration element", e);
            }
        }
    }
}
