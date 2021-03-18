using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;

using Contour.Configuration;

namespace Contour.Sending
{
    /// <summary>
    /// Отправитель сообщений.
    /// </summary>
    public interface ISender : IBusComponent
    {
        /// <summary>
        /// Проверяет возможность создать маршрут для указанной метки сообщения.
        /// </summary>
        /// <param name="label">Метка сообщения, для которой необходимо построить маршрут.</param>
        /// <returns><c>true</c> - если для указанной метки можно построить маршрут.</returns>
        bool CanRoute(MessageLabel label);

        /// <summary>
        /// Отправляет сообщение в формате запрос-ответ.
        /// </summary>
        /// <param name="payload">Сообщение запроса.</param>
        /// <param name="options">Параметры запроса.</param>
        /// <typeparam name="T">Тип сообщения ответа.</typeparam>
        /// <returns>Задача выполнения запроса.</returns>
        [Obsolete("Необходимо использовать метод Request с указанием метки сообщения.")]
        Task<T> Request<T>(object payload, RequestOptions options) where T : class;

        /// <summary>
        /// Отправляет сообщение в формате запрос-ответ.
        /// </summary>
        /// <param name="payload">Сообщение запроса.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <typeparam name="T">Тип сообщения ответа.</typeparam>
        /// <returns>Задача выполнения запроса.</returns>
        [Obsolete("Необходимо использовать метод Request с указанием метки сообщения.")]
        Task<T> Request<T>(object payload, IDictionary<string, object> headers) where T : class;

        /// <summary>
        /// Отправляет одностороннее сообщение.
        /// </summary>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="headers">Заголовки сообщения.</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        [Obsolete("Необходимо использовать метод Send с указанием метки сообщения.")]
        Task Send(object payload, IDictionary<string, object> headers);

        /// <summary>
        /// Отправляет одностороннее сообщение.
        /// </summary>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Заголовки сообщения.</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        [Obsolete("Необходимо использовать метод Send с указанием метки сообщения.")]
        Task Send(object payload, PublishingOptions options);

        /// <summary>
        /// Отправляет сообщение в формате запрос-ответ.
        /// </summary>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="payload">Сообщение запроса.</param>
        /// <param name="options">Параметры запроса.</param>
        /// <typeparam name="T">Тип сообщения ответа.</typeparam>
        /// <returns>Задача выполнения запроса.</returns>
        Task<T> Request<T>(MessageLabel label, object payload, RequestOptions options) where T : class;

        /// <summary>
        /// Отправляет сообщение в формате запрос-ответ.
        /// </summary>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="payload">Сообщение запроса.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <typeparam name="T">Тип сообщения ответа.</typeparam>
        /// <returns>Задача выполнения запроса.</returns>
        Task<T> Request<T>(MessageLabel label, object payload, IDictionary<string, object> headers) where T : class;

        /// <summary>
        /// Отправляет одностороннее сообщение.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="headers">Заголовки сообщения.</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        Task Send(MessageLabel label, object payload, IDictionary<string, object> headers);

        /// <summary>
        /// Отправляет одностороннее сообщение c определенным connection, если сообщение с таким лейблом нельзя отправить через такой коннекшен, то будет выброшено исключение
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="headers">Заголовки сообщения.</param>
        /// <param name="connectionKey">Идентификатор подключения, по которому нужно отправить сообщение</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        Task Send(MessageLabel label, object payload, IDictionary<string, object> headers, string connectionKey);

        /// <summary>
        /// Отправляет одностороннее сообщение.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Заголовки сообщения.</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        Task Send(MessageLabel label, object payload, PublishingOptions options);
    }
}
