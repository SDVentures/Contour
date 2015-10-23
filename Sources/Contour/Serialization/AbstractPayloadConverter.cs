// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractPayloadConverter.cs" company="">
//   
// </copyright>
// <summary>
//   The abstract payload converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Serialization
{
    using System;

    // TODO: not used yet, should use some Payload container
    /// <summary>
    /// The abstract payload converter.
    /// </summary>
    public abstract class AbstractPayloadConverter : IPayloadConverter
    {
        #region Public Properties

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public abstract string ContentType { get; }

        #endregion

        #region Public Methods and Operators

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
            if (payload.GetType() == typeof(byte[]))
            {
                return (byte[])payload;
            }

            return null;
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
            if (targetType == typeof(byte[]))
            {
                return payload;
            }

            return null;
        }

        #endregion
    }
}
