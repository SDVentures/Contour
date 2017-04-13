﻿using System.Collections.Generic;
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
        /// Отправляет одностороннее сообщение.
        /// </summary>
        /// <param name="label">Метка отправляемого сообщения.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="options">Заголовки сообщения.</param>
        /// <returns>Задача выполнения отправки сообщения.</returns>
        Task Send(MessageLabel label, object payload, PublishingOptions options);
    }
}
