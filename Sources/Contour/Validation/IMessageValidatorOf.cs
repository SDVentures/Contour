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
    }
}
