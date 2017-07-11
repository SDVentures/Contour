namespace Contour.Caching
{
    using Contour.Configuration;
    using Contour.Configuration.Configurator;
    using Contour.Filters;

    public class CachingConfigurator : IConfigurator
    {
        private readonly ConfigHelper configHelper;
        
        public CachingConfigurator(ConfigHelper configHelper)
        {
            this.configHelper = configHelper;
        }

        /// <inheritdoc />
        public IBusConfigurator Configure(string endpointName, IBusConfigurator currentConfiguration)
        {
            var outgoingConfiguration = this.configHelper.GetOutgoingCacheConfig(endpointName);

            var cachingFilter = new CachingFilterDecorator(outgoingConfiguration);

            currentConfiguration.RegisterDecoratorOf<SendingExchangeFilter>(cachingFilter);
            
            return currentConfiguration;
        }

        /// <inheritdoc />
        public string GetEvent(string endpointName, string key)
        {
            return null;
        }

        /// <inheritdoc />
        public IRequestConfiguration GetRequestConfig(string endpointName, string key)
        {
            return null;
        }
    }
}