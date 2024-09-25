using System;

using Contour.Configuration;
using Contour.Helpers;

namespace Contour.Sending
{
    /// <summary>
    /// Настройки отправителя.
    /// </summary>
    public class SenderOptions : EndpointOptions
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SenderOptions"/>.
        /// </summary>
        public SenderOptions()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SenderOptions"/>.
        /// </summary>
        /// <param name="parent">Базовые настройки.</param>
        public SenderOptions(BusOptions parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Требуется ли подтверждение об успешной отправке.
        /// </summary>
        public Maybe<bool> ConfirmationIsRequired { protected get; set; }

        /// <summary>
        /// Требуется ли сохранение сообщения.
        /// </summary>
        public Maybe<bool> Persistently { protected get; set; }

        /// <summary>
        /// Время ожидания ответа на запрос.
        /// </summary>
        public Maybe<TimeSpan?> RequestTimeout { protected get; set; }

        /// <summary>
        /// Построитель вычислителя маршрута.
        /// </summary>
        public Maybe<Func<IRouteResolverBuilder, IRouteResolver>> RouteResolverBuilder { protected get; set; }

        /// <summary>
        /// Время жизни сообщения.
        /// </summary>
        public Maybe<TimeSpan?> Ttl { protected get; set; }

        /// <summary>
        /// Требуется ли отправка сообщений с задержкой
        /// </summary>
        public bool Delayed {  get; set; }

        public bool Direct {  get; set; }

        /// <summary>
        /// Время ожидания подтверждения получения сообщения, null если требуется бесконечное ожидание.
        /// </summary>
        public TimeSpan? ConfirmationTimeout { get; set; }

        /// <summary>
        /// Создает новые настройки отправителя, которые наследуют существующие.
        /// </summary>
        /// <returns>Новые настройки отправителя, наследующие существующие настройки.</returns>
        public override BusOptions Derive()
        {
            return new SenderOptions(this);
        }

        /// <summary>
        /// Возвращает время ожидания ответа на запрос.
        /// </summary>
        /// <returns>
        /// Время ожидания ответа на запрос.
        /// </returns>
        public Maybe<TimeSpan?> GetRequestTimeout()
        {
            return this.Pick<SenderOptions, TimeSpan?>((o) => o.RequestTimeout);
        }

        /// <summary>
        /// Возвращает построитель вычислителя маршрута.
        /// </summary>
        /// <returns>Построитель вычислителя маршрута.</returns>
        public Maybe<Func<IRouteResolverBuilder, IRouteResolver>> GetRouteResolverBuilder()
        {
            return this.Pick<SenderOptions, Func<IRouteResolverBuilder, IRouteResolver>>((o) => o.RouteResolverBuilder);
        }

        /// <summary>
        /// Возвращает время жизни сообщения.
        /// </summary>
        /// <returns>Время жизни сообщения.</returns>
        public Maybe<TimeSpan?> GetTtl()
        {
            return this.Pick<SenderOptions, TimeSpan?>((o) => o.Ttl);
        }

        /// <summary>
        /// Требуется ли подтверждение отправки сообщения.
        /// </summary>
        /// <returns>Подтверждение отправки сообщения.</returns>
        public Maybe<bool> IsConfirmationRequired()
        {
            return this.Pick<SenderOptions, bool>((o) => o.ConfirmationIsRequired);
        }

        /// <summary>
        /// Необходимо ли сохранять сообщение.
        /// </summary>
        /// <returns>Если <c>true</c>, тогда нужно сохранять сообщение, иначе - <c>false</c>.</returns>
        public Maybe<bool> IsPersistently()
        {
            return this.Pick<SenderOptions, bool>((o) => o.Persistently);
        }
    }
}
