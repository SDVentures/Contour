using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Contour
{
    internal sealed class MessageHeaderStorage : IIncomingMessageHeaderStorage
    {
        private static readonly AsyncLocal<Dictionary<string, object>> storage = new AsyncLocal<Dictionary<string, object>>();

        private readonly HashSet<string> blackHeaderSet;

        public MessageHeaderStorage(IEnumerable<string> blackHeaders)
        {
            this.blackHeaderSet = new HashSet<string>(blackHeaders);
        }

        /// <summary>
        /// Сохраняет заголовки входящего сообщения.
        /// </summary>
        /// <param name="headers">Заголовки входящего сообщения.</param>
        public void Store(IDictionary<string, object> headers)
        {
            var refindedHeaders = headers
                .Where(p => !this.blackHeaderSet.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);
            storage.Value = refindedHeaders;
        }

        /// <summary>
        /// Возвращает сохраненные заголовки входящего сообщения.
        /// </summary>
        /// <returns>Заголовки входящего сообщения.</returns>
        public IDictionary<string, object> Load()
        {
            return storage.Value;
        }

        /// <summary>
        /// Registers header names to be excluded on storing
        /// </summary>
        /// <param name="headers">Header names</param>
        public void RegisterExcludedHeaders(IEnumerable<string> headers)
        {
            this.blackHeaderSet.UnionWith(headers);
        }

        /// <summary>
        /// Возвращает сохраненные заголовки входящего сообщения.
        /// </summary>
        /// <returns>Заголовки входящего сообщения.</returns>
        public static IDictionary<string, object> LoadStatic()
        {
            return storage.Value;
        }

    }
}
