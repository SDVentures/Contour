namespace Contour
{
    /// <summary>
    ///   Методы расширения строки.
    /// </summary>
    public static class StringEx
    {
        #region Public Methods and Operators

        /// <summary>
        /// Форматирует строку с использованием параметров.
        /// </summary>
        /// <param name="s">
        /// Шаблон строки.
        /// </param>
        /// <param name="args">
        /// Параметры для подстановки.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string FormatEx(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        /// <summary>
        /// Преобразует строку в метку сообщения <see cref="MessageLabel"/>.
        /// </summary>
        /// <param name="s">
        /// Текстовое представление метки.
        /// </param>
        /// <returns>
        /// Строго типизированная метка сообщения.
        /// </returns>
        public static MessageLabel ToMessageLabel(this string s)
        {
            return MessageLabel.From(s);
        }

        #endregion
    }
}
