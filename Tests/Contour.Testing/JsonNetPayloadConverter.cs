using System;
using System.Text;
using Contour.Serialization;
using Newtonsoft.Json;

namespace Contour.Testing
{
    /// <summary>
    /// The json net payload converter.
    /// </summary>
    public class JsonNetPayloadConverter : IPayloadConverter
    {
        /// <summary>
        /// Gets the content type.
        /// </summary>
        public string ContentType
        {
            get
            {
                return "application/json";
            }
        }

        /// <summary>
        /// The from object.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        public byte[] FromObject(object payload)
        {
            string json = JsonConvert.SerializeObject(payload);

            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// The to object.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ToObject(byte[] payload, Type targetType)
        {
            string decoded = Encoding.UTF8.GetString(payload);

            return JsonConvert.DeserializeObject(decoded, targetType);
        }
    }
}
