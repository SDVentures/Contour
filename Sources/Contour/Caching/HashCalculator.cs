using System;
using System.Security.Cryptography;

using Contour.Serialization;

namespace Contour.Caching
{
    internal class HashCalculator : IHashCalculator
    {
        private readonly Lazy<IPayloadConverter> payloadConverter;

        private readonly SHA256Managed sha256Managed;

        public HashCalculator(Lazy<IPayloadConverter> payloadConverter)
        {
            this.payloadConverter = payloadConverter;
            this.sha256Managed = new SHA256Managed();
        }

        public string CalculateHash(IMessage message)
        {
            var buffer = this.payloadConverter.Value.FromObject(message.Payload);
            var hash = this.sha256Managed.ComputeHash(buffer);
            return BitConverter.ToString(hash);
        }
    }
}