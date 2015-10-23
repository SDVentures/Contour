namespace Contour
{
    using System.Collections.Generic;

    /// <summary>
    ///   Контейнер для сообщения.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets the headers.
        /// </summary>
        IDictionary<string, object> Headers { get; }

        /// <summary>
        ///   Метка сообщения.
        /// </summary>
        MessageLabel Label { get; }

        /// <summary>
        ///   Содержимое сообщения.
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// Создает копию сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">
        /// Новая метка сообщения.
        /// </param>
        /// <returns>
        /// Новое сообщение.
        /// </returns>
        IMessage WithLabel(MessageLabel label);

        /// <summary>
        /// Создает копию сообщения с указанным содержимым.
        /// </summary>
        /// <typeparam name="T">Тип содержимого.</typeparam>
        /// <param name="payload">Содержимое сообщения.</param>
        /// <returns>Новое сообщение.</returns>
        IMessage WithPayload<T>(T payload) where T : class;
    }
}
