namespace Contour.Caching.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MemoryCacheFixture
    {
        [Test(Description = "cached value should be returned")]
        public void TestCacheIsSet()
        {
            var hasherMock = new Mock<IHasher>();
            hasherMock.Setup(h => h.GetHash(It.IsAny<IMessage>()))
                .Returns("hash");
            var cache = new MemoryCache(hasherMock.Object);
            var key = new Message<string>(MessageLabel.From("label"), "payload");
            var value = "value";

            cache.Set(key, value, TimeSpan.FromMinutes(1.0));
            var containsCachedValue = cache.ContainsKey(key);
            var cachedValue = containsCachedValue ? cache[key] : null;
            
            Assert.IsTrue(containsCachedValue, "cache should contain cached value");
            Assert.AreEqual(value, cachedValue, $"cached value should be equal to '{value}'");

            // teardown because of using MemoryCache.Default
            cache.Set(key, string.Empty, TimeSpan.Zero);
        }

        [Test(Description = "cached value should be returned")]
        public void TestValueIsNotInCache()
        {
            var hasherMock = new Mock<IHasher>();
            hasherMock.Setup(h => h.GetHash(It.IsAny<IMessage>()))
                .Returns("non-existing-hash");
            var cache = new MemoryCache(hasherMock.Object);
            var key = new Message<string>(MessageLabel.From("label"), "payload");

            var containsCachedValue = cache.ContainsKey(key);
            var cachedValue = containsCachedValue ? cache[key] : null;

            Assert.IsFalse(containsCachedValue, "cache should not contain value not cached before");
            Assert.IsNull(cachedValue, "value for not cached key should be null'");

            // teardown because of using MemoryCache.Default
            cache.Set(key, string.Empty, TimeSpan.Zero);
        }
    }
}