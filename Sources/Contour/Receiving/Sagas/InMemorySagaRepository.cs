using System.Collections.Concurrent;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Хранилище саг в памяти.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class InMemorySagaRepository<TS, TK> : ISagaRepository<TS, TK>
    {
        private readonly ConcurrentDictionary<TK, TS> sagas = new ConcurrentDictionary<TK, TS>();

        private readonly ISagaFactory<TS, TK> sagaFactory;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InMemorySagaRepository{TS,TK}"/>. 
        /// </summary>
        /// <param name="sagaFactory">Фабрика саг используемая для создания саги возвращаемой из хранилища.</param>
        public InMemorySagaRepository(ISagaFactory<TS, TK> sagaFactory)
        {
            this.sagaFactory = sagaFactory;
        }

        /// <summary>
        /// Получает сохраненную сагу.
        /// </summary>
        /// <param name="sagaId">Идентификатор запрашиваемой саги.</param>
        /// <returns>Запрашиваемая сага.</returns>
        public ISagaContext<TS, TK> Get(TK sagaId)
        {
            TS sagaData;
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (sagaId == null || !this.sagas.TryGetValue(sagaId, out sagaData))
            {
                return null;
            }

            return this.sagaFactory.Create(sagaId, sagaData);
        }

        /// <summary>
        /// Сохраняет сагу.
        /// </summary>
        /// <param name="sagaContext">Сохраняемая сага.</param>
        public void Store(ISagaContext<TS, TK> sagaContext)
        {
            this.sagas.AddOrUpdate(sagaContext.SagaId, sagaContext.Data, (s, list) => list);
        }

        /// <summary>
        /// Удаляет сагу.
        /// </summary>
        /// <param name="sagaContext">Удаляемая сага.</param>
        public void Remove(ISagaContext<TS, TK> sagaContext)
        {
            TS sagaData;
            this.sagas.TryRemove(sagaContext.SagaId, out sagaData);
        }
    }
}
