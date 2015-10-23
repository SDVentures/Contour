// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageValidatorOf.cs" company="">
//   
// </copyright>
// <summary>
//   Валидатор сообщения.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Validation
{
    /// <summary>
    /// Валидатор сообщения.
    /// </summary>
    /// <typeparam name="T">
    /// Тип содержимого сообщения.
    /// </typeparam>
    public interface IMessageValidatorOf<T> : IMessageValidator
        where T : class
    {
        #region Public Methods and Operators

        /// <summary>
        /// Проверить валидность сообщения.
        /// </summary>
        /// <param name="message">
        /// Сообщение для проверки.
        /// </param>
        /// <returns>
        /// Результат валидации.
        /// </returns>
        ValidationResult Validate(Message<T> message);

        #endregion
    }
}
