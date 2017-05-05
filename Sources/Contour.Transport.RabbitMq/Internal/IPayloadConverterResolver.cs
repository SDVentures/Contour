using Contour.Serialization;

namespace Contour.Transport.RabbitMq.Internal
{
    internal interface IPayloadConverterResolver
    {
        IPayloadConverter ResolveConverter(string contentType);
    }
}