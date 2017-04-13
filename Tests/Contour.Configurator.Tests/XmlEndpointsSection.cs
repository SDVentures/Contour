using Contour.Configuration.Configurator;

namespace Contour.Configurator.Tests
{
    using System.IO;
    using System.Xml;

    /// <summary>
    /// The xml endpoints section.
    /// </summary>
    internal class XmlEndpointsSection : EndpointsSection
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="XmlEndpointsSection"/>.
        /// </summary>
        /// <param name="configXml">
        /// The config xml.
        /// </param>
        public XmlEndpointsSection(string configXml)
        {
            var reader = new XmlTextReader(new StringReader(configXml));

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            this.DeserializeSection(reader);

            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }
    }
}
