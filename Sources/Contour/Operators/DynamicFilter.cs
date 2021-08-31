using System;

using Contour.Receiving;
using Contour.Receiving.Consumers;

namespace Contour.Operators
{
    /// <summary>
    /// Динамический фильтр. 
    /// Выполняет вычисление необходимости фильтрации на основе конфигурационных сообщений от других участников.
    /// </summary>
    /// <typeparam name="T">Тип сохраняемого признака фильтрации.</typeparam>
    public class DynamicFilter<T> : Filter
        where T : class
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DynamicFilter{T}"/>. 
        /// </summary>
        /// <param name="routeFilter">Функция вычисления применяемого правила маршрутизации.</param>
        /// <param name="storage">Хранилище правил маршрутизации.</param>
        public DynamicFilter(Func<IMessage, IKeyValueStorage<T>, bool> routeFilter, IKeyValueStorage<T> storage)
            : base(message => routeFilter(message, storage))
        {
        }

        /// <summary>
        /// Обработчик управляющих сообщений.
        /// </summary>
        /// <typeparam name="TV">Тип правила маршрутизации.</typeparam>
        public class DynamicFilterControlConsumer<TV> : IConsumerOf<T>
            where TV : class
        {
            private readonly Action<IMessage, IKeyValueStorage<TV>> createRoute;

            private readonly IKeyValueStorage<TV> storage;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="DynamicFilterControlConsumer{TV}"/>. 
            /// </summary>
            /// <param name="createRoute">Функция вычисления правила маршрутизации.</param>
            /// <param name="storage">Хранилище правил маршрутизации.</param>
            public DynamicFilterControlConsumer(Action<IMessage, IKeyValueStorage<TV>> createRoute, IKeyValueStorage<TV> storage)
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
