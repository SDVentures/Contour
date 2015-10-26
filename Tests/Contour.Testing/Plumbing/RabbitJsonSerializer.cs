using RestSharp;
using RestSharp.Serializers;

namespace Contour.Testing.Plumbing
{
    internal class RabbitJsonSerializer : ISerializer
    {
        public RabbitJsonSerializer()
        {
            this.ContentType = "application/json";
        }

        public string ContentType { get; set; }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj)
        {
            return SimpleJson.SerializeObject(obj, new RabbitJsonSerializerStrategy());
        }
    }
}
