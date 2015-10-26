using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Создает контекст саги для выполнения шага саги.
    /// </summary>
    /// <typeparam name="TS">Тип данных саги.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class LambdaSagaFactory<TS, TK> : ISagaFactory<TS, TK>
    {
        private readonly Func<TK, ISagaContext<TS, TK>> factoryById;

        private readonly Func<TK, TS, ISagaContext<TS, TK>> factoryByData;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaSagaFactory{TS,TK}"/>. 
        /// </summary>
        /// <param name="factoryById">Создает новый контекст саги с указанным идентификатором.</param>
        /// <param name="factoryByData">Создает новый контекст саги с указанным идентификатором и данными.</param>
        public LambdaSagaFactory(Func<TK, ISagaContext<TS, TK>> factoryById, Func<TK, TS, ISagaContext<TS, TK>> factoryByData)
        {
            this.factoryById = factoryById;
            this.factoryByData = factoryByData;
        }

        /// <summary>
        /// Создает сагу на основе переданного идентификатора.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <returns>Созданная сага.</returns>
        public ISagaContext<TS, TK> Create(TK sagaId)
        {
            return this.factoryById(sagaId);
        }

        /// <summary>
        /// Создает сагу на основе идентификатора и состояния саги.
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <param name="data">Состояние саги.</param>
        /// <returns>Созданная сага.</returns>
        public ISagaContext<TS, TK> Create(TK sagaId, TS data)
        {
            return this.factoryByData(sagaId, data);
        }
    }
}
