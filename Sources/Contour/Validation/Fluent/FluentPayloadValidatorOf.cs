// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentPayloadValidatorOf.cs" company="">
//   
// </copyright>
// <summary>
//   The fluent payload validator of.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Validation.Fluent
{
    using FluentValidation;

    /// <summary>
    /// The fluent payload validator of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class FluentPayloadValidatorOf<T> : AbstractValidator<T>, IMessageValidatorOf<T>
        where T : class
    {
        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public ValidationResult Validate(Message<T> message)
        {
            return this.ValidatePayload(message.Payload);
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public ValidationResult Validate(IMessage message)
        {
            return this.ValidatePayload((T)message.Payload);
        }

        /// <summary>
        /// The validate payload.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public ValidationResult ValidatePayload(T payload)
        {
            return this.Validate(payload).
                ToNative();
        }
    }
}
