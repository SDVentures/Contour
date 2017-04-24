using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Contour.Configuration;
using Contour.Sending;

namespace Contour
{
    /// <summary>
    ///   Клиент шины сообщений.
    ///   Используется для управления циклом жизни клиента шины, а также для передачи сообщений.
    /// </summary>
    public interface IBus : IBusContext, IDisposable
    {
        /// <summary>
        ///   Событие подключения к брокеру
        /// </summary>
        [Obsolete("Bus is no longer responsible for connection handling")]
        event Action<IBus, EventArgs> Connected;

        /// <summary>
        ///   Событие разрыва соединения с брокером
        /// </summary>
        [Obsolete("Bus is no longer responsible for connection handling")]
        event Action<IBus, EventArgs> Disconnected;

        /// <summary>
        ///   Событие окончания запуска шины
        /// </summary>
        event Action<IBus, EventArgs> Started;

        /// <summary>
        ///   Событие начала запуска шины
        /// </summary>
        event Action<IBus, EventArgs> Starting;

        /// <summary>
        ///   Событие окончания остановки шины
        /// </summary>
        event Action<IBus, EventArgs> Stopped;

        /// <summary>
        ///   Событие начала остановки шины
        /// </summary>
        event Action<IBus, EventArgs> Stopping;

        /// <summary>
        ///   Конфигурация шины
        /// </summary>
        IBusConfiguration Configuration { get; }

        /// <summary>
        ///   Позволяет определить, запущен ли данный экземпляр шины
        /// </summary>
        bool IsStarted { get; }

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
        new Task Emit<T>(string label, T payload, PublishingOptions options = null) where T : class;

        /// <summary>
        /// Отправляет сообщение с указанной меткой.
        /// </summary>
        /// <typeparam name="T"><c>.NET</c> тип отправляемого сообщения.</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправки.</param>
        /// <returns>Задача ожидания отправки.</returns>
        new Task Emit<T>(MessageLabel label, T payload, PublishingOptions options = null) where T : class;

        /// <summary>
        /// Отправляет сообщение с указанной меткой.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Отправляемое сообщение.</param>
        /// <param name="headers">Заголовки отправляемого сообщения.</param>
        /// <returns>
        /// <returns>Задача ожидания отправки.</returns>
        /// </returns>
        new Task Emit(MessageLabel label, object payload, IDictionary<string, object> headers);

        /// <summary>
        /// Выполняет синхронный запрос данных с указанной меткой.
        /// </summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения. </param>
        new void Request<TRequest, TResponse>(string label, TRequest request, Action<TResponse> responseAction)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// Выполняет синхронный запрос данных с указанной меткой.
        /// </summary>
        /// <typeparam name="TRequest">Тип данных запроса. </typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        new void Request<TRequest, TResponse>(MessageLabel label, TRequest request, Action<TResponse> responseAction)
            where TRequest : class
            where TResponse : class;

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        new void Request<TRequest, TResponse>(string label, TRequest request, RequestOptions options, Action<TResponse> responseAction)
            where TRequest : class
            where TResponse : class;

        /// <summary>Выполняет синхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого запроса.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <param name="responseAction">Действие которое нужно выполнить при получении ответного сообщения.</param>
        new void Request<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options, Action<TResponse> responseAction)
            where TRequest : class
            where TResponse : class;

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки запроса.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        new Task<TResponse> RequestAsync<TRequest, TResponse>(string label, TRequest request, RequestOptions options = null)
            where TRequest : class
            where TResponse : class;

        /// <summary>Выполняет асинхронный запрос данных с указанной меткой.</summary>
        /// <typeparam name="TRequest">Тип данных запроса.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="request">Отправляемое сообщение.</param>
        /// <param name="options">Настройки отправителя.</param>
        /// <returns>Задача получения ответного сообщения.</returns>
        new Task<TResponse> RequestAsync<TRequest, TResponse>(MessageLabel label, TRequest request, RequestOptions options = null)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        ///   Остановка шины и освобождение всех ресурсов.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Конфигурирование и запуск клиента шины.
        /// </summary>
        /// <param name="waitForReadiness">
        /// Необходимо ожидать полной готовности шины.
        /// </param>
        void Start(bool waitForReadiness = true);

        /// <summary>
        ///   Остановка клиента шины.
        /// </summary>
        void Stop();
    }
}
