using System;
using System.Collections.Generic;

using Contour.Caching;
using Contour.Common.Tests.TestImplementations;

using NUnit.Framework;

using TestContext = Contour.Common.Tests.TestImplementations.TestContext;

namespace Contour.Common.Tests
{
    [TestFixture]
    internal class CachingConsumerOfFixture
    {
        [TestCaseSource(nameof(GetCases))]
        public CombinedTestResult TestCacheIsSet(CacheConfiguration config, TestRequest payload)
        {
            // Arrange
            var consumer = new TestConsumer();
            consumer.InitData();
            var message = new Message<TestRequest>(MessageLabel.From("message.label"), payload);
            var context = new TestContext(message);
            var sut = new CachingConsumerOf<TestRequest>(consumer, config);

            // Act
            sut.Handle(context);
            
            // Assert
            var cachedValue = config?.Cache.ContainsKey(message) ?? false ? ((TestResponse)config.Cache[message]).Value : null;
            return new CombinedTestResult(cachedValue, context.Response.Value);
        }

        public IEnumerable<TestCaseData> GetCases()
        {
            yield return new TestCaseData(GetCacheConfig(), new TestRequest { Id = 1 }).Returns(new CombinedTestResult("One"))
                .SetName("not null response should be equal to cached value");
            yield return new TestCaseData(GetCacheConfig(), new TestRequest { Id = -1 }).Returns(new CombinedTestResult(null))
                .SetName("null response should be equal to cached value");

            yield return new TestCaseData(GetCacheConfig(false), new TestRequest { Id = 1 }).Returns(new CombinedTestResult(null, "One"))
                .SetName("cache should be disabled when caching is disabled in configuration");
            yield return new TestCaseData(GetCacheConfig(null), new TestRequest { Id = 1 }).Returns(new CombinedTestResult(null, "One"))
                .SetName("cache should be disabled when ttl is not set in configuration");
            yield return new TestCaseData(null, new TestRequest { Id = 1 }).Returns(new CombinedTestResult(null, "One"))
                .SetName("value should be requested when caching configuration is not set");
            yield return new TestCaseData(new CacheConfiguration(true, TimeSpan.Zero, new AlwaysExpiringCache()), new TestRequest { Id = 1 }).Returns(new CombinedTestResult(null, "One"))
                .SetName("value should be requested when not found in cache");
        }

        private static CacheConfiguration GetCacheConfig(bool enabled = true)
        {
            return new CacheConfiguration(enabled, TimeSpan.Zero, new DictionaryCache());
        }

        private static CacheConfiguration GetCacheConfig(TimeSpan? ttl, bool enabled = true)
        {
            return new CacheConfiguration(enabled, ttl, new DictionaryCache());
        }
    }
}