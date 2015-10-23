namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Гарант того, что фабрика саг используемая по умолчанию будет создаваться в одном экземпляре.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class SingletonSagaFactory<TS, TK>
    {
        private static readonly ISagaFactory<TS, TK> SagaFactory = new DefaultSagaFactory<TS, TK>();

        /// <summary>
        /// Возвращает единственный экземпляр фабрики саги.
        /// </summary>
        public static ISagaFactory<TS, TK> Instance
        {
            get
            {
                return SagaFactory;
            }
        }
    }
}
