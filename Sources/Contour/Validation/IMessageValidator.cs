// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageValidator.cs" company="">
//   
// </copyright>
// <summary>
//   Валидатор сообщения.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Validation
{
    /// <summary>
    ///   Валидатор сообщения.
    /// </summary>
    public interface IMessageValidator
    {
        /// <summary>
        /// Проверить валидность сообщения.
        /// </summary>
        /// <param name="message">
        /// Сообщение для проверки.
        /// </param>
        /// <returns>
        /// Результат валидации.
        /// </returns>
        ValidationResult Validate(IMessage message);
    }
}
