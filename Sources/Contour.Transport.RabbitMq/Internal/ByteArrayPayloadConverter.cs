using System;

using Contour.Serialization;

namespace Contour.Transport.RabbitMq.Internal
{
    internal class ByteArrayPayloadConverter : IPayloadConverter
    {
        public string ContentType { get; }

        public byte[] FromObject(object payload)
        {
            return (byte[])payload;
        }

        public object ToObject(byte[] payload, Type targetType)
        {
            return (object)payload;
        }
    }
}