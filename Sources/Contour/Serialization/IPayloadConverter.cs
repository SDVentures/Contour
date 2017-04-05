// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Конвертер сообщений.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Serialization
{
    using System;

    /// <summary>
    ///   Конвертер сообщений.
    /// </summary>
    public interface IPayloadConverter
    {
        /// <summary>
        /// Gets the content type.
        /// </summary>
        string ContentType { get; }
        /// <summary>
        /// The from object.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        byte[] FromObject(object payload);

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
        object ToObject(byte[] payload, Type targetType);
    }
}
