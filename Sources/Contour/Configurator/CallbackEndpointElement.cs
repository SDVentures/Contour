using Contour.Configurator.Configuration;

namespace Contour.Configurator
{
    using System.Configuration;

    /// <summary>
    /// The callback endpoint element.
    /// </summary>
    internal class CallbackEndpointElement : ConfigurationElement, ICallbackEndpoint
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether default.
        /// </summary>
        [ConfigurationProperty("default")]
        public bool Default
        {
            get
            {
                return (bool)(base["default"]);
            }
        }

        #endregion
    }
}
