namespace Contour.Caching
{
    using System;
    using System.Security.Cryptography;

    using Contour.Serialization;

    public class Sha256Hasher : IHasher
    {
        private readonly IPayloadConverter converter;

        private readonly SHA256Managed sha256Managed;

        public Sha256Hasher(IPayloadConverter converter)
        {
            this.converter = converter;
            this.sha256Managed = new SHA256Managed();
        }

        public string GetHash(IMessage m)
        {
            var objectBytes = this.converter.FromObject(m.Payload);
            var hashBytes = this.sha256Managed.ComputeHash(objectBytes);
            return m.Label + BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }
}