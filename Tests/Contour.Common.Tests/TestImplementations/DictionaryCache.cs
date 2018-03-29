using System;
using System.Collections.Generic;

using Contour.Caching;
using Contour.Serialization;

namespace Contour.Common.Tests.TestImplementations
{
    internal class DictionaryCache : ICache
    {
        private readonly IDictionary<string, object> innerCache = new Dictionary<string, object>();

        private readonly Sha256Hasher hasher = new Sha256Hasher(new JsonNetPayloadConverter());

        public bool ContainsKey(IMessage key)
        {
            return this.innerCache.ContainsKey(this.hasher.GetHash(key));
        }

        public object this[IMessage key] => this.innerCache[this.hasher.GetHash(key)];

        public void Set(IMessage key, object value, TimeSpan ttl)
        {
            this.innerCache[this.hasher.GetHash(key)] = value;
        }
    }
}