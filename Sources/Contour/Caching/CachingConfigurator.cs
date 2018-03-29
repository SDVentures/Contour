using Contour.Configurator;

namespace Contour.Caching
{
    using Contour.Configuration;
    using Contour.Filters;

    public class CachingConfigurator : IConfigurator
    {
        private readonly CacheConfigProvider cacheConfigProvider;
        
        public CachingConfigurator(CacheConfigProvider cacheConfigProvider)
        {
            this.cacheConfigProvider = cacheConfigProvider;
        }

        /// <inheritdoc />
        public IBusConfigurator Configure(string endpointName, IBusConfigurator currentConfiguration)
        {
            var outgoingConfiguration = this.cacheConfigProvider.GetOutgoingCacheConfig(endpointName);

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