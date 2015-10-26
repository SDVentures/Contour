using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Шаги выполнения саги.
    /// </summary>
    /// <typeparam name="TS">Тип данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class LambdaSagaStep<TS, TM, TK> : ISagaStep<TS, TM, TK>
        where TM : class
    {
        private readonly Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaSagaStep{TS,TM,TK}"/>. 
        /// </summary>
        /// <param name="action">Действие выполняемое на указанном шаге саги.</param>
        public LambdaSagaStep(Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Выполняет шаг саги.
        /// </summary>
        /// <param name="sagaContext">Контекст саги доступный на данном шаге выполнения.</param>
        /// <param name="consumingContext">Контекст обработки входящего сообщения.</param>
        public void Handle(ISagaContext<TS, TK> sagaContext, IConsumingContext<TM> consumingContext)
        {
            this.action(sagaContext, consumingContext);
        }
    }
}
