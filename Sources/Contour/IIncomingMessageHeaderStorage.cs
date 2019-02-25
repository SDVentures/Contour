using System.Collections.Generic;

namespace Contour
{
    /// <summary>
    /// Хранилище заголовков входящих сообщений.
    /// </summary>
    public interface IIncomingMessageHeaderStorage
    {
        /// <summary>
        /// Сохраняет заголовки входящего сообщения.
        /// </summary>
        /// <param name="headers">Заголовки входящего сообщения.</param>
        void Store(IDictionary<string, object> headers);

        /// <summary>
        /// Возвращает сохраненные заголовки входящего сообщения.
        /// </summary>
        /// <returns>Заголовки входящего сообщения.</returns>
        IDictionary<string, object> Load();

        /// <summary>
        /// Registers header names to be excluded on storing
        /// </summary>
        /// <param name="headers">Header names</param>
        void RegisterExcludedHeaders(IEnumerable<string> headers);
    }
}
