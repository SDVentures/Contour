using System;

using Contour.Validation;

namespace Contour.Receiving
{
    /// <summary>
    /// Дополнительная конфигурация получателя сообщения.
    /// </summary>
    public interface IReceiverConfiguration
    {
        /// <summary>
        /// Псевдоним получаемого сообщения.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Метка получаемого сообщения.
        /// </summary>
        MessageLabel Label { get; }

        /// <summary>
        /// Настройки получателя сообщений.
        /// </summary>
        ReceiverOptions Options { get; }

        /// <summary>
        /// Регистратор получателей сообщения.
        /// </summary>
        Action<IReceiver> ReceiverRegistration { get; }

        /// <summary>
        /// Механизм проверки сообщений.
        /// </summary>
        IMessageValidator Validator { get; }

        /// <summary>
        /// Проверяет конфигурацию получателя.
        /// </summary>
        void Validate();
    }
}
