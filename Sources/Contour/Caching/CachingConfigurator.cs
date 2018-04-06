using System;

using Common.Logging;

using Contour.Configurator;

namespace Contour.Caching
{
    using Contour.Configuration;
    using Contour.Filters;

    internal class CachingConfigurator : IConfigurator
    {
        private static readonly ILog Log = LogManager.GetLogger<CachingConfigurator>();

        private readonly CacheConfigProvider cacheConfigProvider;
        
        public CachingConfigurator(CacheConfigProvider cacheConfigProvider)
        {
            this.cacheConfigProvider = cacheConfigProvider;
        }

        /// <inheritdoc />
        public IBusConfigurator Configure(string endpointName, IBusConfigurator currentConfiguration)
        {
            var outgoingConfiguration = this.cacheConfigProvider.GetOutgoingCacheConfig(endpointName);

            var cachingFilter = new CachingFilterDecorator(outgoingConfiguration, (currentConfiguration as BusConfiguration)?.MetricsCollector);

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

        /// <inheritdoc />
        public object DecorateConsumer(object consumer, Type messageType, IMessageConfiguration messageConfig, string endpointName)
        {
            var cachingConsumerType = typeof(CachingConsumerOf<>).MakeGenericType(messageType);
            var cacheConfig = this.cacheConfigProvider.GetIncomingCacheConfig(endpointName);

            var consumerCacheConfig = cacheConfig[messageConfig.Label];

            try
            {
                var cachingConsumer = Activator.CreateInstance(cachingConsumerType, consumer, consumerCacheConfig);
                return cachingConsumer;
            }
            catch (Exception e)
            {
                Log.Error(m => m("Could not create caching decorator of '{0}' consumer", messageConfig.Label), e);
                throw;
            }
        }
    }
}