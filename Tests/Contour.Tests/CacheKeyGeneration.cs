using Contour.Caching;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.Tests
{
    public class CacheKeyGeneration
    {
        [TestFixture]
        public class describe_cache_key_provider
        {
            [Test]
            public void should_provide_key_for_given_serializable_object()
            { 
                var obj = new { Name = "abc", Value = "123"};
                var provider = new HashKeyProvider();
                var hash = provider.Get(obj);

                hash.Should().NotBeNullOrEmpty();
            }
        }
    }
}