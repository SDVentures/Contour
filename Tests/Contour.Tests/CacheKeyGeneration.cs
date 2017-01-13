using Contour.Caching;
using NSpec;

namespace Contour.Tests
{
    public class CacheKeyGeneration : NSpec
    {
        public void describe_cache_key_provider()
        {
            it["should provide a key for a given json-serializable object"] = () =>
            {
                var obj = new { Name = "abc", Value = "123"};
                var provider = new HashKeyProvider();
                var hash = provider.Get(obj);

                hash.should_not_be_default();
            };
        }
    }
}