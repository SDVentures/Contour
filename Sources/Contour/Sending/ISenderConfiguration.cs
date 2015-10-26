using Contour.Receiving;

namespace Contour.Sending
{
    /// <summary>
    /// Конфигурация отправителя.
    /// </summary>
    public interface ISenderConfiguration
    {
        /// <summary>
        /// Псевдоним для метки отправляемых сообщений.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Конфигурация получателя ответных сообщений.
        /// </summary>
        IReceiverConfiguration CallbackConfiguration { get; }

        /// <summary>
        /// Метка отправляемых сообщений.
        /// </summary>
        MessageLabel Label { get; }

        /// <summary>
        /// Настройки отправителя.
        /// </summary>
        SenderOptions Options { get; }

        /// <summary>
        /// Определяет необходимо ли использовать обратный вызов для обработки ответных сообщений.
        /// </summary>
        bool RequiresCallback { get; }

        /// <summary>
        /// Проверяет корректность конфигурации отправителя.
        /// </summary>
        void Validate();
    }
}
