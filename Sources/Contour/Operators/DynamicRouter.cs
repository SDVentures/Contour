using System;
using System.Collections.Generic;

using Contour.Receiving;
using Contour.Receiving.Consumers;

namespace Contour.Operators
{
    /// <summary>
    /// Динамический маршрутизатор. 
    /// Выполняет вычисление маршрута на основе конфигурационных сообщений от других участников.
    /// <a href="http://www.eaipatterns.com/DynamicRouter.html"><c>Dynamic Router</c></a>.
    /// </summary>
    /// <typeparam name="T">Тип сохраняемого правила.</typeparam>
    internal class DynamicRouter<T> : IMessageOperator
        where T : class
    {
        private readonly Func<IMessage, IKeyValueStorage<T>, MessageLabel> routeResolverFunc;

        private readonly IKeyValueStorage<T> storage;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DynamicRouter{T}"/>. 
        /// </summary>
        /// <param name="routeResolverFunc">Функция вычисления применяемого правила маршрутизации.</param>
        /// <param name="storage">Хранилище правил маршрутизации.</param>
        public DynamicRouter(Func<IMessage, IKeyValueStorage<T>, MessageLabel> routeResolverFunc, IKeyValueStorage<T> storage)
        {
            this.routeResolverFunc = routeResolverFunc;
            this.storage = storage;
        }

        /// <summary>
        /// Обрабатывает входящее сообщение, вычисляет для него новый маршрут и отправляет как исходящее сообщение.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        /// <returns>Сообщение с новой меткой.</returns>
        public IEnumerable<IMessage> Apply(IMessage message)
        {
            yield return message.WithLabel(this.routeResolverFunc(message, this.storage));
        }

        /// <summary>
        /// Обработчик управляющих сообщений.
        /// </summary>
        /// <typeparam name="TV">Тип правила маршрутизации.</typeparam>
        public class DynamicRouterControlConsumer<TV> : IConsumerOf<T>
            where TV : class
        {
            private readonly Action<IMessage, IKeyValueStorage<TV>> createRoute;

            private readonly IKeyValueStorage<TV> storage;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="DynamicRouterControlConsumer{TV}"/>. 
            /// </summary>
            /// <param name="createRoute"> Функция вычисления правила маршрутизации. </param>
            /// <param name="storage"> Хранилище правил маршрутизации. </param>
            public DynamicRouterControlConsumer(Action<IMessage, IKeyValueStorage<TV>> createRoute, IKeyValueStorage<TV> storage)
            {
                this.createRoute = createRoute;
                this.storage = storage;
            }

            /// <summary>
            /// Обрабатывает управляющие сообщения с целью корректировки правил маршрутизации. 
            /// </summary>
            /// <param name="context">Контекст полученного сообщения.</param>
            public void Handle(IConsumingContext<T> context)
            {
                this.createRoute(context.Message, this.storage);
                context.Accept();
            }
        }
    }
}
