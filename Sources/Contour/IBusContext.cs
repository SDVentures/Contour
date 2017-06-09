using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Contour.Sending;
using Contour.Serialization;

namespace Contour
{
    /// <summary>
    ///   Контекст шины сообщений.
    ///   Используется для передачи и получения сообщений.
    /// </summary>
    public interface IBusContext
    {
        /// <summary>
        ///   Описание экземпляра шины
        /// </summary>
        IEndpoint Endpoint { get; }

        /// <summary>
        ///   Установлен при полной готовности шины.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Определяет метку сообщения.
        /// </summary>
        IMessageLabelHandler MessageLabelHandler { get; }

        /// <summary>
        /// Преобразует тело сообщения, в двоичное представление.
        /// </summary>
        IReadOnlyCollection<IPayloadConverter> PayloadConverters { get; }

        /// <summary>
        /// Признак полной готовности шины.
        /// </summary>
        WaitHandle WhenReady { get; }

        /// <summary>
        /// Проверяет возможность получения сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns>
        /// Если <c>true</c> - тогда шина может обрабатывать сообщения с такой меткой, иначе - <c>false</c>.
        /// </returns>
        bool CanHandle(string label);

        /// <summary>
        /// Проверяет возможность получения сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns>
        /// Если <c>true</c> - тогда шина может обрабатывать сообщения с такой меткой, иначе - <c>false</c>.
        /// </returns>
        bool CanHandle(MessageLabel label);

        /// <summary>
        /// Проверяет возможность отправки с использованием указанной метки.
        /// </summary>
        /// <param name="label">Метка сообщения.</param>
        /// <returns>
        /// Если <c>true</c> - тогда шина может отправлять сообщения с такой меткой, иначе - <c>false</c>.
        /// </returns>
        bool CanRoute(string label);

        /// <summary>
        /// Проверяет возможность отправки с использованием указанной метки.
        /// </summary>
        /// <param name="label">
        /// Метка сообщения.
        /// </param>
        /// <returns>
        /// Если <c>true</c> - тогда шина может отправлять сообщения с такой меткой, иначе - <c>false</c>.
        /// </returns>
        bool CanRoute(MessageLabel label);

        /// <summary>
        /// Отправляет сообщение с указанной меткой.
        /// </summary>
        /// <remarks>Метка сообщения должна быть зарегистрирована.</remarks>
        /// <typeparam name="T"><c>.NET</c> тип отправляемого сообщения.
        /// </typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <returns>
        /// Задача ожидания отправки.
        /// </returns>
        Task Emit<T>(string label, T payload, PublishingOptions options = null) where T : class;

        /// <summary>
        /// Отправляет сообщение с указанной меткой.
        /// </summary>
        /// <typeparam name="T"><c>.NET</c> тип отправляемого сообщения.</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправки.</param>
        /// <returns>Задача ожидания отправки.</returns>
        Task Emit<T>(MessageLabel label, T payload, PublishingOptions options = null) where T : class;

        /// <summary>
        /// Отправляет сообщение с указанной меткой.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки отправляемого сообщения.</param>
        /// <returns>
        /// <returns>Задача ожидания отправки.</returns>
        /// </returns>
        Task Emit(MessageLabel label, object payload, IDictionary<string, object> headers);

        /// <summary>
        /// Выполняет синхронный запрос данных с указанной меткой.
        /// </summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения. </param>
        void Request<TRequest, TResponse>(string label, TRequest request, Action<TResponse> responseAction) where TRequest : class where TResponse : class;

        /// <summary>
        /// Выполняет синхронный запрос данных с указанной меткой.
        /// </summary>
        /// <typeparam name="TRequest">Тип данных запроса. </typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        void Request<TRequest, TResponse>(MessageLabel label, TRequest request, Action<TResponse> responseAction) where TRequest : class where TResponse : class;

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        void Request<TRequest, TResponse>(string label, TRequest request, RequestOptions options, Action<TResponse> responseAction) where TRequest : class where TResponse : class;

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        void Request<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options, Action<TResponse> responseAction) where TRequest : class where TResponse : class;

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        void Request<TRequest, TResponse>(MessageLabel label, TRequest request, IDictionary<string, object> headers, Action<TResponse> responseAction)
            where TRequest : class
            where TResponse : class;

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки запроса.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(string label, TRequest request, RequestOptions options = null) where TRequest : class where TResponse : class;

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options = null) where TRequest : class where TResponse : class;

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки запроса.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(MessageLabel label, TRequest request, IDictionary<string, object> headers)
            where TRequest : class
            where TResponse : class;
    }
}
