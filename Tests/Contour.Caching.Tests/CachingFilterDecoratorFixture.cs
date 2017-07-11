namespace Contour.Caching.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Contour.Caching.Tests.TestImplementations;
    using Contour.Filters;

    using NUnit.Framework;

    [TestFixture]
    internal class CachingFilterDecoratorFixture
    {
        private const string RouteName = "command.outgoing.request.get";

        private const int RequestId = 0;

        private const string ResponseValue = "Zero";

        [TestCaseSource(nameof(GetCases))]
        public object TestCacheIsSet(IDictionary<string, CacheConfiguration> cacheConfiguration, MessageExchange exchange, TestResponse response)
        {
            // Arrange
            var responseMessage = new Message<TestResponse>(MessageLabel.From(RouteName), response);
            var sendFilter = new SendingExchangeFilter(
                e =>
                    {
                        e.In = responseMessage;
                        var tcs = new TaskCompletionSource<bool>();
                        tcs.SetResult(true);
                        return tcs.Task;
                    });
            var sut = new CachingFilterDecorator(cacheConfiguration);

            // Act
            sut.Process(sendFilter, exchange, new MessageExchangeFilterInvoker(new[] { sendFilter }, new Dictionary<Type, IMessageExchangeFilterDecorator> { { sendFilter.GetType(), sut } })).Wait();

            // Assert
            var cache = cacheConfiguration[exchange.Out.Label.Name].Cache;
            var cached = cache.ContainsKey(exchange.Out) ? ((TestResponse)cache[exchange.Out]).Value : null;
            return new CombinedTestResult(cached, response.Value);
        }

        public IEnumerable<TestCaseData> GetCases()
        {
            yield return new TestCaseData(GetCacheConfig(), GetExchange(), GetResponse()).Returns(new CombinedTestResult(ResponseValue))
                .SetName("not null response should be equal to cached value");
            var config = GetCacheConfig();
            config[RouteName].Cache.Set(new Message<TestRequest>(MessageLabel.From(RouteName), GetRequest()), GetResponse(), TimeSpan.Zero);
            yield return new TestCaseData(config, GetExchange(), new TestResponse { Value = "NotFromCacheResponse" }).Returns(new CombinedTestResult(ResponseValue, "NotFromCacheResponse"))
                .SetName("cached value should be returned if set");

            yield return new TestCaseData(GetCacheConfig(false), GetExchange(), GetResponse()).Returns(new CombinedTestResult(null, ResponseValue))
                .SetName("cache should be disabled when caching is disabled in configuration");
            yield return new TestCaseData(GetCacheConfig(null), GetExchange(), GetResponse()).Returns(new CombinedTestResult(null, ResponseValue))
                .SetName("cache should be disabled when ttl is not set in configuration");
            yield return new TestCaseData(GetCacheConfig(null), new MessageExchange(new Message(MessageLabel.From(RouteName), new TestRequest())), GetResponse()).Returns(new CombinedTestResult(null, ResponseValue))
                .SetName("cache should not be accessed when emitting non-request message");
            yield return new TestCaseData(GetCacheConfig(new AlwaysExpiringCache(), TimeSpan.Zero), GetExchange(), GetResponse()).Returns(new CombinedTestResult(null, ResponseValue))
                .SetName("value should be requested when not found in cache");
        }

        private static TestResponse GetResponse()
        {
            return new TestResponse { Value = ResponseValue };
        }

        private static MessageExchange GetExchange(string routeName = null)
        {
            var outMessage = new Message<TestRequest>(MessageLabel.From(!string.IsNullOrEmpty(routeName) ? routeName : RouteName), GetRequest());

            var exchange = new MessageExchange(outMessage, typeof(TestResponse));
            
            return exchange;
        }

        private static TestRequest GetRequest()
        {
            return new TestRequest { Id = RequestId };
        }

        private static IDictionary<string, CacheConfiguration> GetCacheConfig(bool enabled = true)
        {
            return GetCacheConfig(TimeSpan.Zero, enabled);
        }

        private static IDictionary<string, CacheConfiguration> GetCacheConfig(TimeSpan? ttl, bool enabled = true)
        {
            return GetCacheConfig(new DictionaryCache(), ttl, enabled);
        }

        private static IDictionary<string, CacheConfiguration> GetCacheConfig(ICache cache, TimeSpan? ttl, bool enabled = true)
        {
            return new Dictionary<string, CacheConfiguration> { { RouteName, new CacheConfiguration(enabled, ttl, cache) } };
        }
    }
}