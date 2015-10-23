namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Контекст саги на определенном шаге.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class SagaContext<TS, TK> : ISagaContext<TS, TK>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SagaContext{TS,TK}"/>. 
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        public SagaContext(TK sagaId)
        {
            this.SagaId = sagaId;
            this.Data = default(TS);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SagaContext{TS,TK}"/>. 
        /// </summary>
        /// <param name="sagaId">Идентификатор саги.</param>
        /// <param name="data">Пользовательские данные сохраняемые в саге.</param>
        public SagaContext(TK sagaId, TS data)
            : this(sagaId)
        {
            this.Data = data;
        }

        /// <summary>
        /// Идентификатор саги.
        /// </summary>
        public TK SagaId { get; private set; }

        /// <summary>
        /// Если <c>true</c> - тогда сага завершена, иначе - <c>false</c>.
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Пользовательские данные сохраняемые в саге.
        /// </summary>
        public TS Data { get; private set; }

        /// <summary>
        /// Обновляет пользовательские данные сохраняемые в саге.
        /// </summary>
        /// <param name="data">Пользовательские данные сохраняемые в саге.</param>
        public void UpdateData(TS data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Отмечает сагу как завершенную.
        /// </summary>
        public void Complete()
        {
            this.Completed = true;
        }
    }
}
