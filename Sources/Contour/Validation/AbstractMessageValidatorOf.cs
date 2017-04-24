// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractMessageValidatorOf.cs" company="">
//   
// </copyright>
// <summary>
//   The abstract message validator of.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Validation
{
    /// <summary>
    /// The abstract message validator of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class AbstractMessageValidatorOf<T> : IMessageValidatorOf<T>
        where T : class
    {
        #region Public Methods and Operators

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public abstract ValidationResult Validate(Message<T> message);

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
            return this.Validate((Message<T>)message);
        }

        #endregion
    }
}
