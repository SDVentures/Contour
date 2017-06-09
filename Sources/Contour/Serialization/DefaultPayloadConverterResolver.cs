using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Contour.Serialization
{
    internal class DefaultPayloadConverterResolver : IPayloadConverterResolver
    {
        private readonly string firstContentType;
        private readonly IDictionary<string, IPayloadConverter> converters;

        public DefaultPayloadConverterResolver(ReadOnlyCollection<IPayloadConverter> converters)
        {
            this.converters = new Dictionary<string, IPayloadConverter>();
            foreach (var converter in converters)
            {
                this.converters[converter.ContentType] = converter;
            }
            this.firstContentType = converters.First().ContentType;
        }

        public IPayloadConverter ResolveConverter(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return this.converters[this.firstContentType];
            }

            if (!this.converters.ContainsKey(contentType))
            {
                throw new ArgumentOutOfRangeException(nameof(contentType), "there is no converter registered for this content type.");
            }

            return this.converters[contentType];
        }

        public ReadOnlyCollection<IPayloadConverter> Converters => new ReadOnlyCollection<IPayloadConverter>(this.converters.Values.ToList());
    }
}