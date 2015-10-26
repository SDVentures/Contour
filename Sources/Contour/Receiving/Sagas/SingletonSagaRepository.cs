namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Гарантирует, что хранилище саги используемое по умолчанию создается 
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class SingletonSagaRepository<TS, TK>
    {
        private static readonly ISagaRepository<TS, TK> SagaRepository = new InMemorySagaRepository<TS, TK>(SingletonSagaFactory<TS, TK>.Instance);

        /// <summary>
        /// Возвращает единственный экземпляр хранилища саги.
        /// </summary>
        public static ISagaRepository<TS, TK> Instance 
        {
            get
            {
                return SagaRepository;
            }
        }
    }
}
