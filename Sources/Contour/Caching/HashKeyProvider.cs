using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Contour.Caching
{
    public class HashKeyProvider : IKeyProvider
    {
        private readonly HashAlgorithm algorithm;

        public HashKeyProvider() : this(SHA256.Create())
        {
        }

        public HashKeyProvider(HashAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        public string Get(object input)
        {
            var json = JsonConvert.SerializeObject(input);
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.Default))
                {
                    writer.Write(json);

                    var hash = algorithm.ComputeHash(stream);
                    return Convert.ToBase64String(hash);
                }
            }
        }
    }
}