using System.Collections.ObjectModel;

using Contour.Serialization;

namespace Contour
{
    public interface IPayloadConverterResolver
    {
        IPayloadConverter ResolveConverter(string contentType);

        ReadOnlyCollection<IPayloadConverter> Converters { get; }
    }
}