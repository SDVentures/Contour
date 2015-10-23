using System;
using System.Collections.Generic;
using System.Text;

using Contour.Helpers;

namespace Contour
{
    /// <summary>
    /// Заголовки сообщения.
    /// </summary>
    public class Headers
    {
        /// <summary>
        /// Корреляционный идентификатор, нужен для объединения в группу набора сообщений.
        /// </summary>
        public static readonly string CorrelationId = "x-correlation-id";

        /// <summary>
        /// Заголовок, который содержит правила устаревания данных.
        /// Поддерживается следующий формат: <c>x-expires: at 2014-04-01T22:00:33Z</c> или <c>x-expires: in 100</c>.
        /// </summary>
        public static readonly string Expires = "x-expires";

        /// <summary>
        /// Метка сообщения, с которым она была отправлена.
        /// Использование этого заголовка не рекомендуется.
        /// </summary>
        public static readonly string MessageLabel = "x-message-type";

        /// <summary>
        /// Содержит информацию о том, что сообщение необходимо сохранить.
        /// </summary>
        public static readonly string Persist = "x-persist";

        /// <summary>
        /// Адрес ответного сообщения на запрос.
        /// </summary>
        public static readonly string ReplyRoute = "x-reply-route";

        /// <summary>
        /// Время ответа на запрос.
        /// </summary>
        public static readonly string Timeout = "x-timeout";

        /// <summary>
        /// Время жизни сообщения.
        /// </summary>
        public static readonly string Ttl = "x-ttl";

        /// <summary>
        /// Время жизни сообщений в очереди.
        /// </summary>
        public static readonly string QueueMessageTtl = "x-message-ttl";

        /// <summary>
        /// Путь сообщений по системе. Содержит список всех конечных точек, через которое прошло сообщение, разделенное символом ";".
        /// </summary>
        public static readonly string Breadcrumbs = "x-breadcrumbs";

        /// <summary>
        /// Идентификатор сообщения инициировавшего обмен сообщениями.
        /// </summary>
        public static readonly string OriginalMessageId = "x-original-message-id";

        /// <summary>
        /// Получает значение заголовка из сообщения и удаляет его из списка заголовков сообщения.
        /// </summary>
        /// <param name="headers">
        /// Список заголовков сообщения.
        /// </param>
        /// <param name="key">
        /// Заголовок сообщения, чье значение нужно получить.
        /// </param>
        /// <typeparam name="T">Тип получаемого значения.</typeparam>
        /// <returns>Если заголовок существует, тогда его значение, иначе <c>null</c> или 0.</returns>
        public static T Extract<T>(IDictionary<string, object> headers, string key)
        {
            object value;
            if (headers.TryGetValue(key, out value))
            {
                headers.Remove(key);
                return (T)value;
            }

            return default(T);
        }

        /// <summary>
        /// Получает строковое значение заголовка из набора заголовков.
        /// </summary>
        /// <param name="headers">Коллекция заголовков сообщений.</param>
        /// <param name="key">Имя заголовка.</param>
        /// <returns>Строковое значение заголовка или пустая строка, если заголовка не существует в наборе.</returns>
        public static string GetString(IDictionary<string, object> headers, string key)
        {
            object value;
            if (headers.TryGetValue(key, out value))
            {
                if (value is string)
                {
                    return (string)value;
                }

                if (value is byte[])
                {
                    return Encoding.UTF8.GetString((byte[])value);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Применяет к коллекции заголовков установку заголовка <c>Breadcrumbs</c>.
        /// </summary>
        /// <param name="headers">Исходная коллекция заголовков, которая подвергается изменениям.</param>
        /// <param name="endpoint">Имя конечной точки записываемой в заголовок.</param>
        /// <returns>Исходная колллекция заголовков с изменениями.</returns>
        public static IDictionary<string, object> ApplyBreadcrumbs(IDictionary<string, object> headers, string endpoint)
        {
            if (!headers.ContainsKey(Breadcrumbs))
            {
                headers[Breadcrumbs] = endpoint;
            }
            else
            {
                headers[Breadcrumbs] = GetString(headers, Breadcrumbs) + ";" + endpoint;
            }

            return headers;            
        }

        /// <summary>
        /// Применяет к коллекции заголовков установку заголовка <c>OriginalMessageId</c>.
        /// </summary>
        /// <param name="headers">Исходная коллекция заголовков, которая подвергается изменениям.</param>
        /// <returns>Исходная колллекция заголовков с изменениями.</returns>
        public static IDictionary<string, object> ApplyOriginalMessageId(IDictionary<string, object> headers)
        {
            if (!headers.ContainsKey(OriginalMessageId))
            {
                headers[OriginalMessageId] = Guid.NewGuid().ToString("n");
            }

            return headers;
        }

        /// <summary>
        /// Применяет к коллекции заголовков установку заголовка <c>Persist</c>.
        /// </summary>
        /// <param name="headers">Исходная коллекция заголовков, которая подвергается изменениям.</param>
        /// <param name="persistently">Настройки персистентности сообщения.</param>
        /// <returns>Исходная колллекция заголовков с изменениями.</returns>
        public static IDictionary<string, object> ApplyPersistently(IDictionary<string, object> headers, Maybe<bool> persistently)
        {
            if (persistently != null && persistently.HasValue)
            {
                headers[Persist] = persistently.Value;
            }

            return headers;
        }

        /// <summary>
        /// Применяет к коллекции заголовков установку заголовка <c>Ttl</c>.
        /// </summary>
        /// <param name="headers">Исходная коллекция заголовков, которая подвергается изменениям.</param>
        /// <param name="ttl">Настройки персистентности сообщения.</param>
        /// <returns>Исходная колллекция заголовков с изменениями.</returns>
        public static IDictionary<string, object> ApplyTtl(IDictionary<string, object> headers, Maybe<TimeSpan?> ttl)
        {
            if (ttl != null && ttl.HasValue)
            {
                headers[Ttl] = ttl.Value;
            }

            return headers;
        }
    }
}
