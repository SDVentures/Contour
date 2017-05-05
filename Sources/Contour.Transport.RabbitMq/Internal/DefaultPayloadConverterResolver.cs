using System;
using System.Collections.Generic;
using System.Linq;

using Contour.Serialization;

namespace Contour.Transport.RabbitMq.Internal
{
    internal class DefaultPayloadConverterResolver : IPayloadConverterResolver
    {
        private readonly string firstContentType;
        private readonly IDictionary<string, IPayloadConverter> converters;

        public DefaultPayloadConverterResolver(IReadOnlyCollection<IPayloadConverter> converters)
        {
            this.converters = converters.ToDictionary(c => c.ContentType, c => c);
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
                throw new ArgumentOutOfRangeException(nameof(contentType), "There is no a such content type of converters.");
            }

            return this.converters[contentType];
        }
    }
}