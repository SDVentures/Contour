using RestSharp;

namespace Contour.Testing.Plumbing
{
    internal class RabbitJsonSerializerStrategy : PocoJsonSerializerStrategy
    {
        protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
        {
            return base.MapClrMemberNameToJsonFieldName(clrPropertyName.ToLowerInvariant());
        }
    }
}
